﻿
namespace GMap.NET.MapProviders
{
    using System;

    /// <summary>
    /// CzechTuristMap provider, http://www.mapy.cz/
    /// </summary>
    public class CzechTuristWinterMapProvider : CzechMapProviderBase
    {
        public static readonly CzechTuristWinterMapProvider Instance;

        CzechTuristWinterMapProvider()
        {
        }

        static CzechTuristWinterMapProvider()
        {
            Instance = new CzechTuristWinterMapProvider();
        }

        #region GMapProvider Members

        readonly Guid id = new Guid("1A2C354A-BF73-42AC-92E5-90DECE204F11");
        public override Guid Id
        {
            get
            {
                return id;
            }
        }

        readonly string name = "CzechTuristWinterMap";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            // http://m3.mapserver.mapy.cz/wturist_winter-m/14-8802-5528

            return string.Format(UrlFormat, GetServerNum(pos, 3) + 1, zoom, pos.X, pos.Y);
        }

        static readonly string UrlFormat = "http://m{0}.mapserver.mapy.cz/wturist_winter-m/{1}-{2}-{3}";
    }
}