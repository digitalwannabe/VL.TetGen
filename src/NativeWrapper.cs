// For examples, see:
// https://thegraybook.vvvv.org/reference/extending/writing-nodes.html#examples

namespace Geometry.Tetrahedralization.NativeWrapper;

using System;
using System.Runtime.InteropServices;

public static class NativeWrapper
{
        // Specify the name and location of the native DLL
        //private const string DllName = "TetGen2vvvv.dll";

        // Declare the native functions as static extern methods with [DllImport] attribute
        // Use the appropriate calling convention, parameter types, and marshaling options
        [DllImport("TetGen2vvvv.dll")]
        public static extern IntPtr tetCalculate(String behaviour, double[] vertXYZ, double[] vertAttr, int[] vertMarker, int[] numPoly, int[] numVertices, int[] vertIndex, int[] numFHoles, double[] fHoleXYZ, int[] facetMarker, double[] HoleXYZ, double[] RegionXYZ, double[] RegionAttrib, double[] RegionVolConst, int[] binSizes, String fileName);

}

 public class NativeAdapter
    {
        // You can use the same name and signature as the native functions
        // Just call the wrapper methods internally
        public static IntPtr tetCalculate(String behaviour, double[] vertXYZ, double[] vertAttr, int[] vertMarker, int[] numPoly, int[] numVertices, int[] vertIndex, int[] numFHoles, double[] fHoleXYZ, int[] facetMarker, double[] HoleXYZ, double[] RegionXYZ, double[] RegionAttrib, double[] RegionVolConst, int[] binSizes, String fileName)
        {
            return NativeWrapper.tetCalculate(behaviour, vertXYZ, vertAttr, vertMarker, numPoly, numVertices, vertIndex, numFHoles, fHoleXYZ, facetMarker, HoleXYZ, RegionXYZ,RegionAttrib,RegionVolConst,binSizes, fileName);
        }

    }