using System;


namespace ImageWatchCSharp
{
    public readonly  struct MatType : IEquatable<int>
    {
   
        public int Value => value;

        public int Depth => value & 7;

        public bool IsInteger => Depth < 5;

        public int Channels => (Value >> 3) + 1;

        //
        //     Entity value
        private readonly int value;

        private const int CV_CN_MAX = 512;

        private const int CV_CN_SHIFT = 3;

        private const int CV_DEPTH_MAX = 8;

        
        public const int CV_8U = 0;

        //     type depth constants
        public const int CV_8S = 1;

        //     type depth constants
        public const int CV_16U = 2;

        //     type depth constants
        public const int CV_16S = 3;

        //     type depth constants
        public const int CV_32S = 4;

        //     type depth constants
        public const int CV_32F = 5;

        //     type depth constants
        public const int CV_64F = 6;

        //     type depth constants
        public const int CV_USRTYPE1 = 7;

        //     predefined type constants
        public static readonly MatType CV_8UC1 = CV_8UC(1);

        //     predefined type constants
        public static readonly MatType CV_8UC2 = CV_8UC(2);

        //     predefined type constants
        public static readonly MatType CV_8UC3 = CV_8UC(3);

        //     predefined type constants
        public static readonly MatType CV_8UC4 = CV_8UC(4);

        //     predefined type constants
        public static readonly MatType CV_8SC1 = CV_8SC(1);

        //     predefined type constants
        public static readonly MatType CV_8SC2 = CV_8SC(2);

        //     predefined type constants
        public static readonly MatType CV_8SC3 = CV_8SC(3);

        //     predefined type constants
        public static readonly MatType CV_8SC4 = CV_8SC(4);

        //     predefined type constants
        public static readonly MatType CV_16UC1 = CV_16UC(1);

        //     predefined type constants
        public static readonly MatType CV_16UC2 = CV_16UC(2);

        //     predefined type constants
        public static readonly MatType CV_16UC3 = CV_16UC(3);

        //     predefined type constants
        public static readonly MatType CV_16UC4 = CV_16UC(4);

        //     predefined type constants
        public static readonly MatType CV_16SC1 = CV_16SC(1);

        //     predefined type constants
        public static readonly MatType CV_16SC2 = CV_16SC(2);

        //     predefined type constants
        public static readonly MatType CV_16SC3 = CV_16SC(3);

        //     predefined type constants
        public static readonly MatType CV_16SC4 = CV_16SC(4);

        //     predefined type constants
        public static readonly MatType CV_32SC1 = CV_32SC(1);

        //     predefined type constants
        public static readonly MatType CV_32SC2 = CV_32SC(2);

        //     predefined type constants
        public static readonly MatType CV_32SC3 = CV_32SC(3);

        //     predefined type constants
        public static readonly MatType CV_32SC4 = CV_32SC(4);

        //     predefined type constants
        public static readonly MatType CV_32FC1 = CV_32FC(1);

        //     predefined type constants
        public static readonly MatType CV_32FC2 = CV_32FC(2);

        //     predefined type constants
        public static readonly MatType CV_32FC3 = CV_32FC(3);

        //     predefined type constants
        public static readonly MatType CV_32FC4 = CV_32FC(4);

        //     predefined type constants
        public static readonly MatType CV_64FC1 = CV_64FC(1);

        //     predefined type constants
        public static readonly MatType CV_64FC2 = CV_64FC(2);

        //     predefined type constants
        public static readonly MatType CV_64FC3 = CV_64FC(3);

        //     predefined type constants
        public static readonly MatType CV_64FC4 = CV_64FC(4);

        //     Matrix data type (depth and number of channels)
        public MatType(int value)
        {
            this.value = value;
        }
        //   self:
        public static explicit operator int(MatType self)
        {
            return self.value;
        }

        public int ToInt32()
        {
            return value;
        }

        //   value:
        public static implicit operator MatType(int value)
        {
            return new MatType(value);
        }

        //   value:
        public static MatType FromInt32(int value)
        {
            return new MatType(value);
        }

        public bool Equals(int other)
        {
            return value == other;
        }

        public static bool operator ==(MatType self, int other)
        {
            return self.Equals(other);
        }

        public static bool operator !=(MatType self, int other)
        {
            return !self.Equals(other);
        }

        public override string ToString()
        {
            string text;
            switch (Depth)
            {
                case 0:
                    text = "CV_8U";
                    break;
                case 1:
                    text = "CV_8S";
                    break;
                case 2:
                    text = "CV_16U";
                    break;
                case 3:
                    text = "CV_16S";
                    break;
                case 4:
                    text = "CV_32S";
                    break;
                case 5:
                    text = "CV_32F";
                    break;
                case 6:
                    text = "CV_64F";
                    break;
                case 7:
                    text = "CV_USRTYPE1";
                    break;
                default:
                    return $"Unsupported type value ({Value})";
            }

            int channels = Channels;
            if (channels <= 4)
            {
                return text + "C" + channels;
            }

            return text + "C(" + channels + ")";
        }

        public static MatType CV_8UC(int ch)
        {
            return MakeType(0, ch);
        }

        public static MatType CV_8SC(int ch)
        {
            return MakeType(1, ch);
        }

        public static MatType CV_16UC(int ch)
        {
            return MakeType(2, ch);
        }

        public static MatType CV_16SC(int ch)
        {
            return MakeType(3, ch);
        }

        public static MatType CV_32SC(int ch)
        {
            return MakeType(4, ch);
        }

        public static MatType CV_32FC(int ch)
        {
            return MakeType(5, ch);
        }

        public static MatType CV_64FC(int ch)
        {
            return MakeType(6, ch);
        }

        public static MatType MakeType(int depth, int channels)
        {
            if (channels <= 0 || channels >= 512)
            {
                throw new Exception("Channels count should be 1.." + 511);
            }

            if (depth < 0 || depth >= 8)
            {
                throw new Exception("Data type depth should be 0.." + 7);
            }

            return (depth & 7) + (channels - 1 << 3);
        }
       
        public void Deconstruct(out int value)
        {
            value = this.value;
        }
    }
}
