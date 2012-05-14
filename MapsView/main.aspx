<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="main.aspx.cs" Inherits="MapsView.main3" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
  <head>
    <meta name="viewport" content="initial-scale=1.0, user-scalable=no" />
    <style type="text/css">
      html { height: 100% }
      body { height: 100%; margin: 0; padding: 0 }
      #map_canvas { height: 100% }
    </style>
    <script type="text/javascript"
      src="http://maps.googleapis.com/maps/api/js?key=AIzaSyCCGcU9w_frNfO2IhsrI666Xb_VT2x7jpo&sensor=false">
    </script>
    <script>
        
        var options = {
            getTileUrl: function (coord, zoom) {               
                //return "http://mw1.google.com/mw-planetary/lunar/lunarmaps_v1/clem_bw" + "/" + zoom + "/" + normalizedCoord.x + "/" + (bound - normalizedCoord.y - 1) + ".jpg";
                return "http://mikecann.s3.amazonaws.com/printomi/printomimaps/data/" + coord.x + "_" + coord.y + "_" + zoom + ".jpg";
                //return "./data/" + coord.x + "_" + coord.y + "_" + zoom + ".jpg"; 
            },
            tileSize: new google.maps.Size(450, 337),
            maxZoom: 8,
            minZoom: 0,
            name: "Printomi"
        };

        var mapType = new google.maps.ImageMapType(options);

        function initialize() {            
                
            var myOptions = {
                center: new google.maps.LatLng(0, 0),
                zoom: 0,
                streetViewControl: false,
                mapTypeControlOptions: { mapTypeIds: ["printomi"] }
            };

            var map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);
            map.mapTypes.set('printomi', mapType);
            map.setMapTypeId('printomi');
        }
       
  </script>
  </head>
  <body onload="initialize()">
    <div id="map_canvas" style="width:100%; height:100%"></div>
  </body>
</html>