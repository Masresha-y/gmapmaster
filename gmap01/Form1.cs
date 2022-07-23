﻿using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AForge.Video;


namespace gmap01
{

    public partial class Form1 : Form
    {

        //Video Stream Reading 
        MJPEGStream streamvideo;
        MJPEGStream streamvideo2;

        private List<PointLatLng> _points;

        System.IO.Ports.SerialPort Port;
        bool isClosed = false;

        public Form1()
        {
            InitializeComponent();

            streamvideo = new MJPEGStream("http://213.34.225.97:8080/mjpg/video.mjpg");
            streamvideo.NewFrame += GetNewframe;

            streamvideo2 = new MJPEGStream("http://185.10.80.33:8082/cgi-bin/faststream.jpg?stream=half&fps=15&rand=COUNTER");
            streamvideo2.NewFrame += GetNewframe2;

            Port = new System.IO.Ports.SerialPort();
            Port.PortName = "COM8";
            Port.BaudRate = 9600;
            Port.ReadTimeout = 500;

            try
            {
                Port.Open();
            }
            catch { }


            _points = new List<PointLatLng>();
        }

        void GetNewframe(object sender, NewFrameEventArgs eventarg)
        {
            Bitmap bmp = (Bitmap)eventarg.Frame.Clone();
            pictureBox14.Image = bmp;
        }

        void GetNewframe2(object sender, NewFrameEventArgs eventarg)
        {
            Bitmap bmp = (Bitmap)eventarg.Frame.Clone();
            pictureBox1.Image = bmp;
        }


        void Form1_Load(object sender, EventArgs e)
        {

            Thread Helo = new Thread(ListenSerial);
            Helo.Start();


            GMapProviders.GoogleMap.ApiKey = @"AIzaSyD5KRIisFJcnRpuPbXnnFuFyIwDoKBgwfU";
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            map.CacheLocation = @"cache";
            map.DragButton = MouseButtons.Left;

            map.MapProvider = GMapProviders.GoogleMap;
            map.ShowCenter = false;
            map.MinZoom = 10;
            map.MaxZoom = 18;
            map.Zoom = 14;
            map.SetPositionByKeywords("Mysuru, India");
            LoadRandomPointsForRoute();
        }

        void LoadRandomPointsForRoute()
        {
            var mysuruPoint = new PointLatLng(8.7406481, 38.949457);
            var bengaluruPoint = new PointLatLng(8.7474347, 38.9856776);
            LoadMap(mysuruPoint);
            _points.Clear();
            _points.Add(mysuruPoint);
            _points.Add(bengaluruPoint);
        }

        void LoadRndPtsForRt()
        {
            LoadMap(new PointLatLng(-10.9393, -37.06274211));
            _points.Clear();
            _points.Add(new PointLatLng(-10.9393, -37.06274211));
            _points.Add(new PointLatLng(-10.9393, -37.06276451));
            _points.Add(new PointLatLng(-10.9393, -37.06284305));
            _points.Add(new PointLatLng(-10.9392, -37.0628786));
            _points.Add(new PointLatLng(-10.9389, -37.0628786));
            _points.Add(new PointLatLng(-10.9385, -37.06283866));
        }

        private void ListenSerial()
        {

            while (!isClosed)
            {
                try
                {
                    //read to data from arduino
                    string AString = Port.ReadLine();

                    string[] splittedArray = AString.Split(',');


                    //write the data in something textbox
                    txtLat.Invoke(new MethodInvoker(
                        delegate
                        {
                            lblLat.Text = splittedArray[1];
                            txtLat.Text = splittedArray[1];
                            lblLongtiude.Text = splittedArray[2];
                            txtLng.Text = splittedArray[2];
                            label11.Text = splittedArray[4]+"KM/HR";

                        }

                        ));

                }
                catch
                {


                }
            }


        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            streamvideo.Stop();
            streamvideo2.Stop();
            Application.Exit();

            isClosed = true;
            if (Port.IsOpen)
                Port.Close();
        }

       

        void LoadRandomPointsForPolygon()
        {
            LoadMap(new PointLatLng(-25.969562, 32.585789));
            _points.Clear();
            _points.Add(new PointLatLng(-25.969562, 32.585789));
            _points.Add(new PointLatLng(-25.966205, 32.588171));
            _points.Add(new PointLatLng(-25.968134, 32.591647));
            _points.Add(new PointLatLng(-25.971684, 32.589759));
        }

        void LoadRandomMysuruPointsForPolygon()
        {            
            LoadMap(new PointLatLng(12.2847403736782, 76.6698932647705));
            _points.Clear();
            _points.Add(new PointLatLng(12.2847403736782, 76.6698932647705));
            _points.Add(new PointLatLng(12.2964812621897, 76.6946125030518));
            _points.Add(new PointLatLng(12.2720763995067, 76.6634559631348));
            _points.Add(new PointLatLng(12.2629344767928, 76.7036247253418));
        }

        void btnLoadIntoMap_Click(object sender, EventArgs e)
        {
            if (!(txtLat.Text.Trim().Equals("") && txtLng.Text.Trim().Equals("")))
            {
                // Reverse Geococding
                var point = new PointLatLng(Convert.ToDouble(txtLat.Text), Convert.ToDouble(txtLng.Text));
                LoadAndMark(point);
                GeoCoderStatusCode statusCode;
                var placeMark = GoogleMapProvider.Instance.GetPlacemark(point, out statusCode);
                if (statusCode == GeoCoderStatusCode.OK)
                {
                    txtAddress.Text = placeMark?.Address;
                }
                else
                {
                    txtAddress.Text = "Something Went Wrong.  Returned Status: " + statusCode;
                }

            }
            else
            {
                // Geocoding
                if (!txtAddress.Text.Trim().Equals(""))
                {
                    GeoCoderStatusCode statusCode;
                    var pointLatLng = GoogleMapProvider.Instance.GetPoint(txtAddress.Text.Trim(), out statusCode);
                    if (statusCode == GeoCoderStatusCode.OK)
                    {

                        txtLat.Text = pointLatLng.Value.Lat.ToString();
                        txtLng.Text = pointLatLng.Value.Lng.ToString();
                        LoadAndMark(pointLatLng.Value);
                    }
                    else
                    {
                        MessageBox.Show("Something Went Wrong.  Returned Status: " + statusCode);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Data To Load");
                }
            }
        }

        void btnAddPoint_Click(object sender, EventArgs e)
        {
            _points.Add(new PointLatLng(Convert.ToDouble(txtLat.Text),
                Convert.ToDouble(txtLng.Text)));
        }

        void btnClearList_Click(object sender, EventArgs e) => _points = new List<PointLatLng>();

        List<int> routeOverlays = new List<int>();
        void btnGetRouteInfo_Click(object sender, EventArgs e)
        {
            // GDirections ss;
            // GMapProviders.GoogleMap.GetDirections(out ss, _points[0], _points[1], false, false, false, false, false);

            var route = GoogleMapProvider.Instance.GetRoute(_points[0], _points[1], false, false, 14);
           // GMapRoute routess = new GMapRoute(__points, "rt");
            GMapRoute routess = new GMapRoute(_points, "A walk in the park");
            routess.Stroke = new Pen(Color.Red, 3);
           
          //  var r = new GMapRoute(ss.Route, "My Route")
          //  {
                //Stroke = new Pen(Color.Red, 5)
          //  };


            var routes = new GMapOverlay("routes");
            routes.Routes.Add(routess);
            map.Overlays.Add(routes);
            //LoadMap(ss.Route[0]);
            map.RefreshMap();
           // lblDistance.Text = ss.Distance;
            routeOverlays.Add(map.Overlays.Count - 1);

        }

        void btnAddPoly_Click(object sender, EventArgs e)
        {
            var polygon = new GMapPolygon(_points, "My Area")
            {
                Stroke = new Pen(Color.DarkGreen, 2),
                Fill = new SolidBrush(Color.BurlyWood)
            };

            //Long_point > Long_LowerLeft + Lat_point * (Long_UpperLeft - Long_LowerLeft) / (Lat_UpperLeft - Lat_LowerLeft)
            var polygons = new GMapOverlay("polygons");
            polygons.Polygons.Add(polygon);
            map.Overlays.Add(polygons);
            map.RefreshMap();
        }



        void btnRemoveOverlay_Click(object sender, EventArgs e)
        {
            if (map.Overlays.Count > 0)
            {
                map.Overlays.RemoveAt(0);
            }
            map.RefreshMap();
        }

        void map_MouseClick(object sender, MouseEventArgs e)
        {
            if (chkMouseClick.Checked && e.Button == MouseButtons.Left)
            {
                var point = map.FromLocalToLatLng(e.X, e.Y);
                var lat = point.Lat;
                var lng = point.Lng;

                txtLat.Text = lat + "";
                txtLng.Text = lng + "";

                // Load Location
                LoadMap(point);

                // Adding Marker
                AddMarker(point);

                // Get Address
                var addresses = GetAddress(point);

                // Display Address
                if (addresses != null)
                    txtAddress.Text = "Address: \n-----------------------\n" + addresses[0];
                else
                    txtAddress.Text = "Unable To Load Address";
                //var res = map.Overlays[1].Polygons[0].IsInside(map.Position);
                //MessageBox.Show(res.ToString());

            }
        }

        void LoadMap(PointLatLng point) => map.Position = point;

        void AddMarker(PointLatLng pointToAdd, GMarkerGoogleType markerType = GMarkerGoogleType.arrow)
        {
            var markers = new GMapOverlay("markers");
            var marker = new GMarkerGoogle(pointToAdd, markerType);
            
            var tooltip = new GMap.NET.WindowsForms.ToolTips.GMapBaloonToolTip(marker)
            {
                Stroke = new Pen(new SolidBrush(Color.Black)),
                Font = new Font("Arial", 9, FontStyle.Bold | FontStyle.Underline),
                Fill = new SolidBrush(Color.Black),
                Foreground = new SolidBrush(Color.White)                
            };
            marker.ToolTip = tooltip;
            marker.ToolTip.TextPadding = new Size(0, 0);
            marker.ToolTip.Format.Alignment = StringAlignment.Far;

            marker.ToolTipText = $"Latitude: {Math.Round(map.Position.Lat, 3)}, \nLongitude: {Math.Round(map.Position.Lng, 3)}";
            

            markers.Markers.Add(marker);
            map.Overlays.Add(markers);
            //map.RefreshMap();
            map.UpdateMarkerLocalPosition(marker);
        }

        private List<string> GetAddress(PointLatLng point)
        {
            List<Placemark> placemarks = null;
            var statusCode = GMapProviders.GoogleMap.GetPlacemarks(point, out placemarks);
            if (statusCode == GeoCoderStatusCode.OK && placemarks != null)
            {
                var addresses = new List<string>();
                foreach (var placemark in placemarks)
                    addresses.Add(placemark.Address);

                return addresses;
            }

            return null;
        }

        private void LoadAndMark(PointLatLng point)
        {
            LoadMap(point);
            AddMarker(point);
        }



        private void btnSearchInsidePoly_Click(object sender, EventArgs e)
        {
            var pointToSearch = new PointLatLng(Convert.ToDouble(txtLat.Text), Convert.ToDouble(txtLng.Text));

            if (!SearchInsidePolygons(pointToSearch) && !SearchInPolygons(pointToSearch))
            {
                MessageBox.Show("Location Not Found");
            }
            else
            {
                MessageBox.Show("Location Found");
            }

        }

        private bool SearchInPolygons(PointLatLng pointToSearch)
        {
            var overlays = map.Overlays;
            foreach (var overlay in overlays)
            {
                var polygons = overlay.Polygons;
                foreach (var polygon in polygons)
                {
                    foreach (var point in polygon.Points)
                    {
                        if (point == pointToSearch)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool SearchInsidePolygons(PointLatLng pointToSearch)
        {
            var overlays = map.Overlays;
            foreach (var overlay in overlays)
            {
                var polygons = overlay.Polygons;

                foreach (var polygon in polygons)
                {
                    if (polygon.IsInside(pointToSearch))
                    {
                        polygon.Fill = new SolidBrush(Color.FromArgb(120, 10, 200, 100));
                        LoadMap(pointToSearch);
                        AddMarker(pointToSearch, GMarkerGoogleType.lightblue);
                        map.RefreshMap();
                        return true;
                    }
                }
            }
            return false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            streamvideo.Start();
        }

        private void btnStartCam2_Click(object sender, EventArgs e)
        {
            streamvideo2.Start();
        }
    }
}

