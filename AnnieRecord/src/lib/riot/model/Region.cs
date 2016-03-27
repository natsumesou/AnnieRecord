using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    [Serializable]
    public class Region : BaseModel
    {
        public enum Type { br, eune, euw, kr, lan, las, na, oce, ru, tr, jp };
        public enum Platform { NA1, BR1, LA1, LA2, OC1, EUN1, TR1, RU, EUW1, KR, JP1 };

        public Type type
        {
            get;
            private set;
        }

        public Platform platform
        {
            get;
            private set;
        }

        public Region(Type regionType)
        {
            this.type = regionType;
            switch (this.type)
            {
                case Type.br:
                    this.platform = Platform.BR1;
                    break;
                case Type.eune:
                    this.platform = Platform.EUN1;
                    break;
                case Type.euw:
                    this.platform = Platform.EUW1;
                    break;
                case Type.kr:
                    this.platform = Platform.KR;
                    break;
                case Type.lan:
                    this.platform = Platform.LA1;
                    break;
                case Type.las:
                    this.platform = Platform.LA2;
                    break;
                case Type.na:
                    this.platform = Platform.NA1;
                    break;
                case Type.oce:
                    this.platform = Platform.OC1;
                    break;
                case Type.ru:
                    this.platform = Platform.RU;
                    break;
                case Type.tr:
                    this.platform = Platform.TR1;
                    break;
                case Type.jp:
                    this.platform = Platform.JP1;
                    break;
                default:
                    this.platform = Platform.NA1;
                    break;
            }
        }

        public static Region fromPlatformString(String platformStr)
        {
            Type t;
            switch (platformStr)
            {
                case "NA1":
                    t = Type.na;
                    break;
                case "BR1":
                    t = Type.br;
                    break;
                case "LA1":
                    t = Type.lan;
                    break;
                case "LA2":
                    t = Type.las;
                    break;
                case "OC1":
                    t = Type.oce;
                    break;
                case "EUN1":
                    t = Type.eune;
                    break;
                case "TR1":
                    t = Type.tr;
                    break;
                case "RU":
                    t = Type.ru;
                    break;
                case "EUW1":
                    t = Type.euw;
                    break;
                case "KR":
                    t = Type.kr;
                    break;
                case "JP1":
                    t = Type.jp;
                    break;
                default:
                    t = Type.na;
                    break;
            }
            return new Region(t);
        }
    }
}
