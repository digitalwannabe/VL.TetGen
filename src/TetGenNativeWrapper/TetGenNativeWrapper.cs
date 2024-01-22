// For examples, see:
// https://thegraybook.vvvv.org/reference/extending/writing-nodes.html#examples

using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using VL.Lib.Collections;
using Vector3 = Stride.Core.Mathematics.Vector3;

namespace Geometry.TetGen;

public static class TetGenNativeWrapper
{

    /// dll import
    [System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]
    private static extern IntPtr tetCalculate(String behaviour, double[] vertXYZ, double[] vertAttr, int[] vertMarker, int[] numPoly, int[] numVertices, int[] vertIndex, int[] numFHoles, double[] fHoleXYZ, int[] facetMarker, double[] HoleXYZ, double[] RegionXYZ, double[] RegionAttrib, double[] RegionVolConst, int[] binSizes, String fileName);

    [System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]
    private static extern void getValues([In, Out] double[] _vertXYZ, [In, Out] int[] _triIndices, [In, Out] int[] _tetIndices, [In, Out] double[] _regionMarker, [In, Out] int[] _pointMarker, [In, Out] int[] _faceMarker, [In, Out] int[] _pointAttr, [In, Out] int[] _neighborList);

    [System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]

    private static extern int ReleaseMemory(IntPtr ptr);



    public static void WrapTetGen(IEnumerable<Vector3> vertices, IEnumerable<int> indices, IEnumerable<int> vertexIDs,
                                       int holeCount, IEnumerable<Vector3> holeIndicators, 
                                       int regionCount, IEnumerable<Vector3> regionIndicators, IEnumerable<double> regionAttributes, IEnumerable<double> regionVolumeConstraints,
                                       IEnumerable<int> polygonCount, IEnumerable<int> vertexCount,
                                       IEnumerable<int> facetHoleCount, IEnumerable<Vector3> facetHoleIndicators, IEnumerable<int> facetIDs,
                                       string fileName, bool writeInputToFile, bool writeOutputToFile, string behaviour, /*bool run,*/
                                       out IEnumerable<Vector3> verticesOut, out IEnumerable<int> triangleIndices, out IEnumerable<int> tetrahedronIndices,
                                       out IEnumerable<int> vertexIDsOut, out IEnumerable<int> facetIDsOut, out IEnumerable<double> regionAttributesOut, out IEnumerable<int> terahedronNeighbors,
                                       out string Status)
    {


        SpreadBuilder<Vector3> v = new SpreadBuilder<Vector3>();
        SpreadBuilder<int> triI = new SpreadBuilder<int>();
        SpreadBuilder<int> tetI = new SpreadBuilder<int>();
        SpreadBuilder<int> vid = new SpreadBuilder<int>();
        SpreadBuilder<int> fid = new SpreadBuilder<int>();
        SpreadBuilder<double> ra = new SpreadBuilder<double>();
        SpreadBuilder<int> tn = new SpreadBuilder<int>();

        Status = "OK";

        //if (run)
        //{

            int computeNeighbors = Convert.ToInt32(behaviour.Contains("n"));
            int computeRegionAttr = Convert.ToInt32(behaviour.Contains("A"));

            //			//always compute neighbors
            //			behaviour +="n";
            //			FDebug[0]=computeRegionAttr;
            int entries = vertices.Count();
            int entriesXYZ = entries * 3;

            int numFacets = polygonCount.Count();
            int writeIn = Convert.ToInt32(writeInputToFile);
            int writeOut = Convert.ToInt32(writeOutputToFile);

            var help = new Helpers();

            double[] V = new double[entriesXYZ];
            V = help.Vector3DToArray(V, vertices);

            int[] nP = new int[numFacets];
            nP = polygonCount.ToArray();

            int numVert = vertexCount.Count();                                            
            int[] nV = new int[numVert];
            nV = vertexCount.ToArray();

            int numFHolesXYZ = facetHoleIndicators.Count() * 3;
            double[] FfHI = new double[numFHolesXYZ];   //test	
            FfHI = help.Vector3DToArray(FfHI, facetHoleIndicators);

            int numIndices = indices.Count();
            int[] VI = new int[numIndices];
            VI = indices.ToArray();
            for (int nInd = 0; nInd < numIndices; nInd++) VI[nInd] += 1;//tetgen expects indices starting at 1

            //facet marker
            int[] FM = new int[numFacets];
            help.SpreadToArray(FM, facetIDs); //can not use toArray() here, as Slicecount may be smaller than numFacets

            //facet holes
            int[] FfH = new int[numFacets];
            help.SpreadToArray(FfH, facetHoleCount); //can not use toArray() here, as Slicecount may be smaller than numFacets //RECHECK FOR GAMMA

            //hole indicators
            int sizeHI = holeIndicators.Count() * 3;
            double[] HI = new double[sizeHI];
            HI = help.Vector3DToArray(HI, holeIndicators);

            int sizeRI = regionIndicators.Count() * 3;
            double[] RI = new double[sizeRI];
            RI = help.Vector3DToArray(RI, regionIndicators);

            int sizeRA = regionAttributes.Count();
            double[] RA = new double[sizeRA];
            RA = regionAttributes.ToArray();

            int sizeRVC = regionVolumeConstraints.Count();
            double[] RVC = new double[sizeRVC];
            RVC = regionVolumeConstraints.ToArray();

            //point Markers
            int[] PM = new int[entries];
            help.SpreadToArray(PM, vertexIDs); //can not use toArray() here, as Slicecount may be smaller than entries

            //point Attributes
            double[] PA = new double[entries];
            //			help.SpreadToArray(PA,FPA[binID]);//can not use toArray() here, as Slicecount may be smaller than entries	

            int[] settings = new int[8];
            settings[0] = entries;
            settings[1] = numFacets;
            settings[2] = holeCount;
            settings[3] = regionCount;
            settings[4] = writeIn;
            settings[5] = writeOut;
            settings[6] = computeNeighbors;
            settings[7] = computeRegionAttr;

            

            try
            {

                IntPtr tet = tetCalculate(behaviour, V, PA, PM, nP, nV, VI, FfH, FfHI, FM, HI, RI, RA, RVC, settings, fileName);
                int size = 5;
                int[] tetArr = new int[size];
                Marshal.Copy(tet, tetArr, 0, size);

                int nOfPoints = tetArr[0];
                int nOfFaces = tetArr[1];
                int nOfTet = tetArr[2];
                int nOfTetAttr = tetArr[3];
                int nOfPointAttr = tetArr[4];


                int nOfTriIndices = nOfFaces * 3;
                int nOfTetIndices = nOfTet * 4;

                double[] _vertXYZ = new double[nOfPoints * 3];
                int[] _triIndices = new int[nOfTriIndices];
                int[] _tetIndices = new int[nOfTetIndices];
                double[] _regionAttr = new double[nOfTet];
                int[] _pointMarker = new int[nOfPoints];
                int[] _faceMarker = new int[nOfFaces];
                int[] _pointAttr = new int[nOfPoints];
                int[] _neighborList = new int[nOfTetIndices];


                getValues(_vertXYZ, _triIndices, _tetIndices, _regionAttr, _pointMarker, _faceMarker, _pointAttr, _neighborList);


                for (int i = 0; i < nOfPoints; i++)
                {
                    v.Add(new Vector3((float)_vertXYZ[i * 3], (float)_vertXYZ[i * 3 + 1], (float)_vertXYZ[i * 3 + 2]));
                    vid.Add(_pointMarker[i]);
                    //					for (int j = 0; j < nOfPointAttr; j++){ //more than 1 attribute possible? what for?
                    //						FPtAttr[binID][i]= _pointAttr[i];
                    //						}

                }
                for (int i = 0; i < nOfFaces; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        triI.Add(_triIndices[i * 3 + j] - 1);
                    }
                    fid.Add(_faceMarker[i]);
                }
                for (int i = 0; i < nOfTet; i++)
                {
                    if (computeRegionAttr > 0)
                    {
                        for (int j = 0; j < nOfTetAttr; j++)
                        { //more than 1 attribute possible? what for?
                            ra.Add(_regionAttr[i]);
                        }
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        tetI.Add(_tetIndices[i * 4 + j] - 1);
                        if (computeNeighbors > 0)
                        {
                            tn.Add(_neighborList[i * 4 + j] - 1);
                        }

                    }
                }

                ReleaseMemory(tet);
            }
            catch (Exception ex)
            {
                Status = ex.ToString();
            }

            finally
            {

            }

        //}

        verticesOut = v;
        triangleIndices = triI;
        tetrahedronIndices = tetI;
        vertexIDsOut = vid;
        facetIDsOut = fid;
        regionAttributesOut = ra;
        terahedronNeighbors = tn;



    }


    /// HELPERS
    /// 

    public class Helpers
    {
        public double[] Vector3DToArray(double[] V, IEnumerable<Vector3> VertexSequence)
        {
            Vector3[] v = VertexSequence.ToArray<Vector3>();

            for (int i = 0; i < v.Count(); i++)
            {
                V[i * 3] = v[i].X;
                V[i * 3 + 1] = v[i].Y;
                V[i * 3 + 2] = v[i].Z;
            }
            return V;
        }

        public void SpreadToArray<T>(T[] I, IEnumerable<T> Sequence)
        {
            T[] v = Sequence.ToArray<T>();
            for (int i = 0; i < v.Count(); i++)
            {
                I[i] = v[i];
            }
        }

    }
}