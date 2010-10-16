﻿
namespace GMap.NET.Projections
{
   using System;

   /// <summary>
   /// GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]
   /// PROJCS["LKS92 / Latvia TM",GEOGCS["LKS92",DATUM["D_Latvia_1992",SPHEROID["GRS_1980",6378137,298.257222101]],PRIMEM["Greenwich",0],UNIT["Degree",0.017453292519943295]],PROJECTION["Transverse_Mercator"],PARAMETER["latitude_of_origin",0],PARAMETER["central_meridian",24],PARAMETER["scale_factor",0.9996],PARAMETER["false_easting",500000],PARAMETER["false_northing",-6000000],UNIT["Meter",1]]
   /// </summary>
   public class LKS92Projection : PureProjection
   {
      static readonly double MinLatitude = 55.55;
      static readonly double MaxLatitude = 58.58;
      static readonly double MinLongitude = 20.22;
      static readonly double MaxLongitude = 28.28;
      static readonly double orignX = 5120900;
      static readonly double orignY = 3998100;

      Size tileSize = new Size(256, 256);
      public override Size TileSize
      {
         get
         {
            return tileSize;
         }
      }

      public override double Axis
      {
         get
         {
            return 6378137;
         }
      }

      public override double Flattening
      {
         get
         {
            return (1.0 / 298.257222101);
         }
      }

      public override Point FromLatLngToPixel(double lat, double lng, int zoom)
      {
         Point ret = Point.Empty;

         lat = Clip(lat, MinLatitude, MaxLatitude);
         lng = Clip(lng, MinLongitude, MaxLongitude);

         double[] lks = new double[] { lng, lat };
         lks = DTM10(lks);
         lks = MTD10(lks);
         lks = DTM00(lks);

         double res = GetTileMatrixResolution(zoom);

         ret.X = (int) Math.Floor((lks[0] + orignX) / res);
         ret.Y = (int) Math.Floor((orignY - lks[1]) / res);

         return ret;
      }

      public override PointLatLng FromPixelToLatLng(int x, int y, int zoom)
      {
         PointLatLng ret = PointLatLng.Empty;

         double res = GetTileMatrixResolution(zoom);

         double[] lks = new double[] { (x * res) - orignX, -(y * res) + orignY };
         lks = MTD11(lks);
         lks = DTM10(lks);
         lks = MTD10(lks);

         ret.Lat = Clip(lks[1], MinLatitude, MaxLatitude);
         ret.Lng = Clip(lks[0], MinLongitude, MaxLongitude);

         return ret;
      }

      double[] DTM10(double[] lonlat)
      {
         double es;              // Eccentricity squared : (a^2 - b^2)/a^2
         double semiMajor = 6378137.0;		// major axis
         double semiMinor = 6356752.3142451793;		// minor axis
         double ab;				// Semi_major / semi_minor
         double ba;				// Semi_minor / semi_major
         double ses;             // Second eccentricity squared : (a^2 - b^2)/b^2

         es = 1.0 - (semiMinor * semiMinor) / (semiMajor * semiMajor); //e^2
         ses = (Math.Pow(semiMajor, 2) - Math.Pow(semiMinor, 2)) / Math.Pow(semiMinor, 2);
         ba = semiMinor / semiMajor;
         ab = semiMajor / semiMinor;

         // ...

         double lon = DegreesToRadians(lonlat[0]);
         double lat = DegreesToRadians(lonlat[1]);
         double h = lonlat.Length < 3 ? 0 : lonlat[2].Equals(Double.NaN) ? 0 : lonlat[2];
         double v = semiMajor / Math.Sqrt(1 - es * Math.Pow(Math.Sin(lat), 2));
         double x = (v + h) * Math.Cos(lat) * Math.Cos(lon);
         double y = (v + h) * Math.Cos(lat) * Math.Sin(lon);
         double z = ((1 - es) * v + h) * Math.Sin(lat);
         return new double[] { x, y, z, };
      }

      double[] MTD10(double[] pnt)
      {
         const double COS_67P5 = 0.38268343236508977;    // cosine of 67.5 degrees 
         const double AD_C = 1.0026000;                  // Toms region 1 constant 

         double es;                             // Eccentricity squared : (a^2 - b^2)/a^2
         double semiMajor = 6378137.0;		    // major axis
         double semiMinor = 6356752.3141403561;	// minor axis
         double ab;				// Semi_major / semi_minor
         double ba;				// Semi_minor / semi_major
         double ses;            // Second eccentricity squared : (a^2 - b^2)/b^2

         es = 1.0 - (semiMinor * semiMinor) / (semiMajor * semiMajor); //e^2
         ses = (Math.Pow(semiMajor, 2) - Math.Pow(semiMinor, 2)) / Math.Pow(semiMinor, 2);
         ba = semiMinor / semiMajor;
         ab = semiMajor / semiMinor;

         // ...

         bool AtPole = false; // is location in polar region
         double Z = pnt.Length < 3 ? 0 : pnt[2].Equals(Double.NaN) ? 0 : pnt[2];

         double lon = 0;
         double lat = 0;
         double Height = 0;
         if(pnt[0] != 0.0)
         {
            lon = Math.Atan2(pnt[1], pnt[0]);
         }
         else
         {
            if(pnt[1] > 0)
            {
               lon = Math.PI / 2;
            }
            else
               if(pnt[1] < 0)
               {
                  lon = -Math.PI * 0.5;
               }
               else
               {
                  AtPole = true;
                  lon = 0.0;
                  if(Z > 0.0) // north pole
                  {
                     lat = Math.PI * 0.5;
                  }
                  else
                     if(Z < 0.0) // south pole
                     {
                        lat = -Math.PI * 0.5;
                     }
                     else // center of earth
                     {
                        return new double[] { RadiansToDegrees(lon), RadiansToDegrees(Math.PI * 0.5), -semiMinor, };
                     }
               }
         }
         double W2 = pnt[0] * pnt[0] + pnt[1] * pnt[1]; // Square of distance from Z axis
         double W = Math.Sqrt(W2); // distance from Z axis
         double T0 = Z * AD_C; // initial estimate of vertical component
         double S0 = Math.Sqrt(T0 * T0 + W2); // initial estimate of horizontal component
         double Sin_B0 = T0 / S0; // sin(B0), B0 is estimate of Bowring aux variable
         double Cos_B0 = W / S0; // cos(B0)
         double Sin3_B0 = Math.Pow(Sin_B0, 3);
         double T1 = Z + semiMinor * ses * Sin3_B0; // corrected estimate of vertical component
         double Sum = W - semiMajor * es * Cos_B0 * Cos_B0 * Cos_B0; // numerator of cos(phi1)
         double S1 = Math.Sqrt(T1 * T1 + Sum * Sum); // corrected estimate of horizontal component
         double Sin_p1 = T1 / S1; // sin(phi1), phi1 is estimated latitude
         double Cos_p1 = Sum / S1; // cos(phi1)
         double Rn = semiMajor / Math.Sqrt(1.0 - es * Sin_p1 * Sin_p1); // Earth radius at location
         if(Cos_p1 >= COS_67P5)
         {
            Height = W / Cos_p1 - Rn;
         }
         else
            if(Cos_p1 <= -COS_67P5)
            {
               Height = W / -Cos_p1 - Rn;
            }
            else
            {
               Height = Z / Sin_p1 + Rn * (es - 1.0);
            }

         if(!AtPole)
         {
            lat = Math.Atan(Sin_p1 / Cos_p1);
         }
         return new double[] { RadiansToDegrees(lon), RadiansToDegrees(lat), Height, };
      }

      double[] DTM00(double[] lonlat)
      {
         double scaleFactor = 0.9998;	                // scale factor				
         double centralMeridian = 0.41887902047863912;	// Center longitude (projection center) */
         double latOrigin = 0.0;	                // center latitude			
         double falseNorthing = -6000000.0;	            // y offset in meters			
         double falseEasting = 500000.0;	        // x offset in meters			
         double semiMajor = 6378137.0;		        // major axis
         double semiMinor = 6356752.3141403561;		// minor axis
         double metersPerUnit = 1.0;

         double e0, e1, e2, e3;	// eccentricity constants		
         double e, es, esp;		// eccentricity constants		
         double ml0;		    // small value m			

         es = 1.0 - Math.Pow(semiMinor / semiMajor, 2);
         e = Math.Sqrt(es);
         e0 = e0fn(es);
         e1 = e1fn(es);
         e2 = e2fn(es);
         e3 = e3fn(es);
         ml0 = semiMajor * mlfn(e0, e1, e2, e3, latOrigin);
         esp = es / (1.0 - es);

         // ...		

         double lon = DegreesToRadians(lonlat[0]);
         double lat = DegreesToRadians(lonlat[1]);

         double delta_lon = 0.0;  // Delta longitude (Given longitude - center)
         double sin_phi, cos_phi; // sin and cos value				
         double al, als;		  // temporary values				
         double c, t, tq;	      // temporary values				
         double con, n, ml;	      // cone constant, small m			

         delta_lon = AdjustLongitude(lon - centralMeridian);
         SinCos(lat, out sin_phi, out cos_phi);

         al = cos_phi * delta_lon;
         als = Math.Pow(al, 2);
         c = esp * Math.Pow(cos_phi, 2);
         tq = Math.Tan(lat);
         t = Math.Pow(tq, 2);
         con = 1.0 - es * Math.Pow(sin_phi, 2);
         n = semiMajor / Math.Sqrt(con);
         ml = semiMajor * mlfn(e0, e1, e2, e3, lat);

         double x = scaleFactor * n * al * (1.0 + als / 6.0 * (1.0 - t + c + als / 20.0 *
             (5.0 - 18.0 * t + Math.Pow(t, 2) + 72.0 * c - 58.0 * esp))) + falseEasting;

         double y = scaleFactor * (ml - ml0 + n * tq * (als * (0.5 + als / 24.0 *
             (5.0 - t + 9.0 * c + 4.0 * Math.Pow(c, 2) + als / 30.0 * (61.0 - 58.0 * t
             + Math.Pow(t, 2) + 600.0 * c - 330.0 * esp))))) + falseNorthing;

         if(lonlat.Length < 3)
            return new double[] { x / metersPerUnit, y / metersPerUnit };
         else
            return new double[] { x / metersPerUnit, y / metersPerUnit, lonlat[2] };
      }

      double[] DTM01(double[] lonlat)
      {
         double es;                             // Eccentricity squared : (a^2 - b^2)/a^2
         double semiMajor = 6378137.0;		    // major axis
         double semiMinor = 6356752.3141403561;	// minor axis
         double ab;				                // Semi_major / semi_minor
         double ba;				                // Semi_minor / semi_major
         double ses;                            // Second eccentricity squared : (a^2 - b^2)/b^2

         es = 1.0 - (semiMinor * semiMinor) / (semiMajor * semiMajor);
         ses = (Math.Pow(semiMajor, 2) - Math.Pow(semiMinor, 2)) / Math.Pow(semiMinor, 2);
         ba = semiMinor / semiMajor;
         ab = semiMajor / semiMinor;

         // ...

         double lon = DegreesToRadians(lonlat[0]);
         double lat = DegreesToRadians(lonlat[1]);
         double h = lonlat.Length < 3 ? 0 : lonlat[2].Equals(Double.NaN) ? 0 : lonlat[2];
         double v = semiMajor / Math.Sqrt(1 - es * Math.Pow(Math.Sin(lat), 2));
         double x = (v + h) * Math.Cos(lat) * Math.Cos(lon);
         double y = (v + h) * Math.Cos(lat) * Math.Sin(lon);
         double z = ((1 - es) * v + h) * Math.Sin(lat);
         return new double[] { x, y, z, };
      }

      double[] MTD01(double[] pnt)
      {
         const double COS_67P5 = 0.38268343236508977; // cosine of 67.5 degrees 
         const double AD_C = 1.0026000;               // Toms region 1 constant 

         double es;                             // Eccentricity squared : (a^2 - b^2)/a^2
         double semiMajor = 6378137.0;		    // major axis
         double semiMinor = 6356752.3142451793;	// minor axis
         double ab;		                        // Semi_major / semi_minor
         double ba;				                // Semi_minor / semi_major
         double ses;                            // Second eccentricity squared : (a^2 - b^2)/b^2

         es = 1.0 - (semiMinor * semiMinor) / (semiMajor * semiMajor);
         ses = (Math.Pow(semiMajor, 2) - Math.Pow(semiMinor, 2)) / Math.Pow(semiMinor, 2);
         ba = semiMinor / semiMajor;
         ab = semiMajor / semiMinor;

         // ...

         bool At_Pole = false; // is location in polar region
         double Z = pnt.Length < 3 ? 0 : pnt[2].Equals(Double.NaN) ? 0 : pnt[2];

         double lon = 0;
         double lat = 0;
         double Height = 0;
         if(pnt[0] != 0.0)
         {
            lon = Math.Atan2(pnt[1], pnt[0]);
         }
         else
         {
            if(pnt[1] > 0)
            {
               lon = Math.PI / 2;
            }
            else
               if(pnt[1] < 0)
               {
                  lon = -Math.PI * 0.5;
               }
               else
               {
                  At_Pole = true;
                  lon = 0.0;
                  if(Z > 0.0) // north pole
                  {
                     lat = Math.PI * 0.5;
                  }
                  else
                     if(Z < 0.0) // south pole
                     {
                        lat = -Math.PI * 0.5;
                     }
                     else // center of earth
                     {
                        return new double[] { RadiansToDegrees(lon), RadiansToDegrees(Math.PI * 0.5), -semiMinor, };
                     }
               }
         }

         double W2 = pnt[0] * pnt[0] + pnt[1] * pnt[1]; // Square of distance from Z axis
         double W = Math.Sqrt(W2);                      // distance from Z axis
         double T0 = Z * AD_C;                // initial estimate of vertical component
         double S0 = Math.Sqrt(T0 * T0 + W2); //initial estimate of horizontal component
         double Sin_B0 = T0 / S0;             // sin(B0), B0 is estimate of Bowring aux variable
         double Cos_B0 = W / S0;              // cos(B0)
         double Sin3_B0 = Math.Pow(Sin_B0, 3);
         double T1 = Z + semiMinor * ses * Sin3_B0; //corrected estimate of vertical component
         double Sum = W - semiMajor * es * Cos_B0 * Cos_B0 * Cos_B0; // numerator of cos(phi1)
         double S1 = Math.Sqrt(T1 * T1 + Sum * Sum); // corrected estimate of horizontal component
         double Sin_p1 = T1 / S1;  // sin(phi1), phi1 is estimated latitude
         double Cos_p1 = Sum / S1; // cos(phi1)
         double Rn = semiMajor / Math.Sqrt(1.0 - es * Sin_p1 * Sin_p1); // Earth radius at location

         if(Cos_p1 >= COS_67P5)
         {
            Height = W / Cos_p1 - Rn;
         }
         else
            if(Cos_p1 <= -COS_67P5)
            {
               Height = W / -Cos_p1 - Rn;
            }
            else
            {
               Height = Z / Sin_p1 + Rn * (es - 1.0);
            }

         if(!At_Pole)
         {
            lat = Math.Atan(Sin_p1 / Cos_p1);
         }
         return new double[] { RadiansToDegrees(lon), RadiansToDegrees(lat), Height, };
      }

      double[] MTD11(double[] p)
      {
         double scaleFactor = 0.9998;	                // scale factor				
         double centralMeridian = 0.41887902047863912;	// Center longitude (projection center) 
         double latOrigin = 0.0;	                   // center latitude			
         double falseNorthing = -6000000.0;	        // y offset in meters			
         double falseEasting = 500000.0;	    // x offset in meters			
         double semiMajor = 6378137.0;		    // major axis
         double semiMinor = 6356752.3141403561;	// minor axis
         double metersPerUnit = 1.0;

         double e0, e1, e2, e3;	// eccentricity constants		
         double e, es, esp;		// eccentricity constants		
         double ml0;		    // small value m

         es = 1.0 - Math.Pow(semiMinor / semiMajor, 2);
         e = Math.Sqrt(es);
         e0 = e0fn(es);
         e1 = e1fn(es);
         e2 = e2fn(es);
         e3 = e3fn(es);
         ml0 = semiMajor * mlfn(e0, e1, e2, e3, latOrigin);
         esp = es / (1.0 - es);

         // ...

         double con, phi;
         double delta_phi;
         long i;
         double sin_phi, cos_phi, tan_phi;
         double c, cs, t, ts, n, r, d, ds;
         long max_iter = 6;

         double x = p[0] * metersPerUnit - falseEasting;
         double y = p[1] * metersPerUnit - falseNorthing;

         con = (ml0 + y / scaleFactor) / semiMajor;
         phi = con;
         for(i = 0; ; i++)
         {
            delta_phi = ((con + e1 * Math.Sin(2.0 * phi) - e2 * Math.Sin(4.0 * phi) + e3 * Math.Sin(6.0 * phi)) / e0) - phi;
            phi += delta_phi;

            if(Math.Abs(delta_phi) <= EPSLoN)
               break;

            if(i >= max_iter)
               throw new ArgumentException("Latitude failed to converge");
         }

         if(Math.Abs(phi) < HALF_PI)
         {
            SinCos(phi, out sin_phi, out cos_phi);
            tan_phi = Math.Tan(phi);
            c = esp * Math.Pow(cos_phi, 2);
            cs = Math.Pow(c, 2);
            t = Math.Pow(tan_phi, 2);
            ts = Math.Pow(t, 2);
            con = 1.0 - es * Math.Pow(sin_phi, 2);
            n = semiMajor / Math.Sqrt(con);
            r = n * (1.0 - es) / con;
            d = x / (n * scaleFactor);
            ds = Math.Pow(d, 2);

            double lat = phi - (n * tan_phi * ds / r) * (0.5 - ds / 24.0 * (5.0 + 3.0 * t +
                10.0 * c - 4.0 * cs - 9.0 * esp - ds / 30.0 * (61.0 + 90.0 * t +
                298.0 * c + 45.0 * ts - 252.0 * esp - 3.0 * cs)));

            double lon = AdjustLongitude(centralMeridian + (d * (1.0 - ds / 6.0 * (1.0 + 2.0 * t +
                c - ds / 20.0 * (5.0 - 2.0 * c + 28.0 * t - 3.0 * cs + 8.0 * esp +
                24.0 * ts))) / cos_phi));

            if(p.Length < 3)
               return new double[] { RadiansToDegrees(lon), RadiansToDegrees(lat) };
            else
               return new double[] { RadiansToDegrees(lon), RadiansToDegrees(lat), p[2] };
         }
         else
         {
            if(p.Length < 3)
               return new double[] { RadiansToDegrees(HALF_PI * Sign(y)), RadiansToDegrees(centralMeridian) };
            else
               return new double[] { RadiansToDegrees(HALF_PI * Sign(y)), RadiansToDegrees(centralMeridian), p[2] };
         }
      }

      #region -- levels info --
      //"spatialReference":{"wkid":3059},"singleFusedMapCache":true,"tileInfo":{"rows":256,"cols":256,"dpi":96,"format":"PNG8","compressionQuality":0,
      //"origin":{"x":-5120900,"y":3998100},"spatialReference":{"wkid":3059},

      //"lods":[{
      //"level":0,"resolution":1587.50317500635,"scale":6000000},
      //{"level":1,"resolution":793.751587503175,"scale":3000000},
      //{"level":2,"resolution":529.167725002117,"scale":2000000},
      //{"level":3,"resolution":264.583862501058,"scale":1000000},
      //{"level":4,"resolution":132.291931250529,"scale":500000},
      //{"level":5,"resolution":52.9167725002117,"scale":200000},
      //{"level":6,"resolution":26.4583862501058,"scale":100000},
      //{"level":7,"resolution":13.2291931250529,"scale":50000},
      //{"level":8,"resolution":6.61459656252646,"scale":25000},
      //{"level":9,"resolution":2.64583862501058,"scale":10000},
      //{"level":10,"resolution":1.32291931250529,"scale":5000},
      //{"level":11,"resolution":0.529167725002117,"scale":2000}]},

      //"initialExtent":{"xmin":290284.5745,"ymin":159644.05,"xmax":785045.1155,"ymax":452176.95,"spatialReference":{"wkid":3059}},
      //"fullExtent":{"xmin":290284.5745,"ymin":159644.05,"xmax":785045.1155,"ymax":452176.95,"spatialReference":{"wkid":3059}},

      //"units":"esriMeters","supportedImageFormatTypes":"PNG24,PNG,JPG,DIB,TIFF,EMF,PS,PDF,GIF,SVG,SVGZ,AI,BMP","documentInfo":{"Title":"ikartelv","Author":"gstanevicius","Comments":"","Subject":"","Category":"","Keywords":"","Credits":""},"capabilities":"Map,Query,Data"}); 
      #endregion

      public static double GetTileMatrixResolution(int zoom)
      {
         double ret = 0;

         switch(zoom)
         {
            #region -- sizes --
            case 0:
            {
               ret = 1587.50317500635;
            }
            break;

            case 1:
            {
               ret = 793.751587503175;
            }
            break;

            case 2:
            {
               ret = 529.167725002117;
            }
            break;

            case 3:
            {
               ret = 264.583862501058;
            }
            break;

            case 4:
            {
               ret = 132.291931250529;
            }
            break;

            case 5:
            {
               ret = 52.9167725002117;
            }
            break;

            case 6:
            {
               ret = 26.4583862501058;
            }
            break;

            case 7:
            {
               ret = 13.2291931250529;
            }
            break;

            case 8:
            {
               ret = 6.61459656252646;
            }
            break;

            case 9:
            {
               ret = 2.64583862501058;
            }
            break;

            case 10:
            {
               ret = 1.32291931250529;
            }
            break;

            case 11:
            {
               ret = 0.529167725002117;
            }
            break;
            #endregion
         }

         return ret;
      }

      public override double GetGroundResolution(int zoom, double latitude)
      {
         return GetTileMatrixResolution(zoom);
      }

      public override Size GetTileMatrixMinXY(int zoom)
      {
         Size ret = Size.Empty;

         switch(zoom)
         {
            #region -- sizes --
            case 0:
            {
               ret = new Size(13, 8);
            }
            break;

            case 1:
            {
               ret = new Size(26, 17);
            }
            break;

            case 2:
            {
               ret = new Size(39, 26);
            }
            break;

            case 3:
            {
               ret = new Size(79, 52);
            }
            break;

            case 4:
            {
               ret = new Size(159, 105);
            }
            break;

            case 5:
            {
               ret = new Size(399, 262);
            }
            break;

            case 6:
            {
               ret = new Size(798, 525);
            }
            break;

            case 7:
            {
               ret = new Size(1597, 1050);
            }
            break;

            case 8:
            {
               ret = new Size(3195, 2101);
            }
            break;

            case 9:
            {
               ret = new Size(7989, 5254);
            }
            break;

            case 10:
            {
               ret = new Size(15978, 10509);
            }
            break;

            case 11:
            {
               ret = new Size(39945, 26273);
            }
            break;
            #endregion
         }

         return ret;
      }

      public override Size GetTileMatrixMaxXY(int zoom)
      {
         Size ret = Size.Empty;

         switch(zoom)
         {
            #region -- sizes --
            case 0:
            {
               ret = new Size(14, 9);
            }
            break;

            case 1:
            {
               ret = new Size(28, 18);
            }
            break;

            case 2:
            {
               ret = new Size(43, 28);
            }
            break;

            case 3:
            {
               ret = new Size(86, 56);
            }
            break;

            case 4:
            {
               ret = new Size(173, 112);
            }
            break;

            case 5:
            {
               ret = new Size(434, 282);
            }
            break;

            case 6:
            {
               ret = new Size(868, 564);
            }
            break;

            case 7:
            {
               ret = new Size(1737, 1129);
            }
            break;

            case 8:
            {
               ret = new Size(3474, 2258);
            }
            break;

            case 9:
            {
               ret = new Size(8686, 5647);
            }
            break;

            case 10:
            {
               ret = new Size(17372, 11294);
            }
            break;

            case 11:
            {
               ret = new Size(43430, 28236);
            }
            break;
            #endregion
         }

         return ret;
      }
   }
}
