namespace OpenWorld
{
    public struct Points
    {
        public int Length;

        public int[] X;
        public int[] Y;
        
        public Points(int r)
        {
            this.Length = r;
            this.X = new int[r];
            this.Y = new int[r];
        }

        public void SetPoint(float x, float y)
        {
            this.X[(int)y] = (int)x;
            this.Y[(int)y] = (int)y;
        }

        public void Resize(int len)
        {
            this.Length = len;
            System.Array.Resize<int>(ref X, len);
            System.Array.Resize<int>(ref Y, len);
        }

        public bool IsMirror()
        {
            return X[Length - 1] == Y[Length - 1];
        }

        public int[] GetXMax(int r)
        {
            int[] result = new int[r + 1];
            for (int i=0; i<Length; i++)
            {
                if (result[Y[i]] < X[i])
                    result[Y[i]] = X[i];
            }
            return result;
        }
    }

    public struct Circle
    {
        static private float ErrorJudge(float x, float y, float r)
        {
            return x*x + y*y - r*r;
        }

        static private float HalfJudge(float x, float y, float r)
        {
            return ErrorJudge(x-0.5f, y+1f, r);
        }

        static private float InsideJudge(float previous, float x, float y)
        {
            return previous + 2 - 2*x + 2*y;
        }

        static private float OutsideJudge(float previous, float y)
        {
            return previous + 1 + 2*y;
        }

        static public int[] SignInversion(int[] array)
        {
            for (int i=0; i<array.Length; i++)
                array[i] = -array[i];
            return array;
        }

        static public int[] Reverse(int[] array)
        {
            int[] result = new int[array.Length];
            System.Array.Copy(array, 0, result, 0, array.Length);
            System.Array.Reverse(result);
            return result;
        }

        static public Points OneEighth(int r)
        {
            float x = (float)r;
            float d = 0;

            int resultLength = 0;
            Points result = new Points(r);
            for (float y=0; y<r; y++)
            {
                if (y == 0)
                    d = HalfJudge(x, y, (float)r);
                else if (d > 0)
                {
                    x -= 1f;
                    d = InsideJudge(d, x, y);
                }
                else
                    d = OutsideJudge(d, y);
                result.SetPoint(x, y);
                if (y >= x)
                {
                    resultLength = (int)y + 1;
                    break;
                }
            }
            result.Resize(resultLength);
            return result;
        }

        static public int[] HalfMax(int r)
        {
            Points quarter;
            Points oneEighth = OneEighth(r);

            int mirrorDiff = oneEighth.IsMirror() ? 1 : 0;
            quarter = new Points(2 * oneEighth.Length - mirrorDiff);
            System.Array.Copy(oneEighth.X, 0, quarter.X, 0, oneEighth.Length);
            System.Array.Copy(Reverse(oneEighth.Y), 0, quarter.X, oneEighth.Length - mirrorDiff, oneEighth.Length);
            System.Array.Copy(oneEighth.Y, 0, quarter.Y, 0, oneEighth.Length);
            System.Array.Copy(Reverse(oneEighth.X), 0, quarter.Y, oneEighth.Length - mirrorDiff, oneEighth.Length);

            int[] half;
            int[] quarterResult = quarter.GetXMax(r);

            half = new int[2 * r + 1];
            System.Array.Copy(Reverse(quarterResult), 0, half, 0, r + 1);
            System.Array.Copy(quarterResult, 0, half, r, r + 1);

            return half;
        }
    }
}
