using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

public static class ArrayTools
{
    public static float[,] MulAdd(float[,] x, float factor, float constant)
    {
        if (x == null) throw new ArgumentNullException();
        float[] result = new float[x.Length];
        for (int i = 0; i < x.GetLength(0); i++)
        {
            for (int j = 0; j < x.GetLength(1); j++)
            {
                x[i, j] = x[i, j] * factor + constant;
            }
        }
        return x;
    }

    public static float[,] Clip(float[,] x, float th)
    {
        if (x == null) throw new ArgumentNullException();
        float[] result = new float[x.Length];
        for (int i = 0; i < x.GetLength(0); i++)
        {
            for (int j = 0; j < x.GetLength(1); j++)
            {
                if (x[i, j] < th)
                {
                    x[i, j] = th;
                }
            }
        }
        return x;
    }
}



public class GaussianBlur
{
    private readonly float[] _values;

    private readonly int _width;
    private readonly int _height;

    private readonly ParallelOptions _pOptions = new ParallelOptions { MaxDegreeOfParallelism = 16 };

    public GaussianBlur(float[,] source)
    {
        _values = new float[source.GetLength(0) * source.GetLength(1)];

        _width = source.GetLength(0);
        _height = source.GetLength(1);

        Buffer.BlockCopy(source, 0, _values, 0, sizeof(float) * _values.Length);
    }

    public float[,] Process(int radial)
    {
        var newValues = new float[_width * _height];

        gaussBlur_4(_values, newValues, radial);
        float[,] dst = new float[_width, _height];
        Buffer.BlockCopy(newValues, 0, dst, 0, sizeof(float) * _values.Length);
        return dst;
    }

    private void gaussBlur_4(float[] source, float[] dest, int r)
    {
        var bxs = boxesForGauss(r, 3);
        boxBlur_4(source, dest, _width, _height, (bxs[0] - 1) / 2);
        boxBlur_4(dest, source, _width, _height, (bxs[1] - 1) / 2);
        boxBlur_4(source, dest, _width, _height, (bxs[2] - 1) / 2);
    }

    private int[] boxesForGauss(float sigma, float n)
    {
        var wIdeal = (float)Math.Sqrt((12 * sigma * sigma / n) + 1);
        var wl = (int)Math.Floor(wIdeal);
        if (wl % 2 == 0) wl--;
        var wu = wl + 2;

        var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
        var m = Math.Round(mIdeal);

        var sizes = new List<int>();
        for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
        return sizes.ToArray();
    }

    private void boxBlur_4(float[] source, float[] dest, int w, int h, int r)
    {
        for (var i = 0; i < source.Length; i++) dest[i] = source[i];
        boxBlurH_4(dest, source, w, h, r);
        boxBlurT_4(source, dest, w, h, r);
    }

    private void boxBlurH_4(float[] source, float[] dest, int w, int h, int r)
    {
        var iar = (float)1 / (r + r + 1);
        Parallel.For(0, h, _pOptions, i =>
        {
            var ti = i * w;
            var li = ti;
            var ri = ti + r;
            var fv = source[ti];
            var lv = source[ti + w - 1];
            var val = (r + 1) * fv;
            for (var j = 0; j < r; j++) val += source[ti + j];
            for (var j = 0; j <= r; j++)
            {
                val += source[ri++] - fv;
                dest[ti++] = val * iar;
            }
            for (var j = r + 1; j < w - r; j++)
            {
                val += source[ri++] - dest[li++];
                dest[ti++] = val * iar;
            }
            for (var j = w - r; j < w; j++)
            {
                val += lv - source[li++];
                dest[ti++] = val * iar;
            }
        });
    }

    private void boxBlurT_4(float[] source, float[] dest, int w, int h, int r)
    {
        var iar = (float)1 / (r + r + 1);
        Parallel.For(0, w, _pOptions, i =>
        {
            var ti = i;
            var li = ti;
            var ri = ti + r * w;
            var fv = source[ti];
            var lv = source[ti + w * (h - 1)];
            var val = (r + 1) * fv;
            for (var j = 0; j < r; j++) val += source[ti + j * w];
            for (var j = 0; j <= r; j++)
            {
                val += source[ri] - fv;
                dest[ti] = val * iar;
                ri += w;
                ti += w;
            }
            for (var j = r + 1; j < h - r; j++)
            {
                val += source[ri] - source[li];
                dest[ti] = val * iar;
                li += w;
                ri += w;
                ti += w;
            }
            for (var j = h - r; j < h; j++)
            {
                val += lv - source[li];
                dest[ti] = val * iar;
                li += w;
                ti += w;
            }
        });
    }
}