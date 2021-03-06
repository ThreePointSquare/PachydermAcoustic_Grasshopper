﻿//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2019, Arthur van der Harten 
//'Pachyderm-Acoustic is free software; you can redistribute it and/or modify 
//'it under the terms of the GNU General Public License as published 
//'by the Free Software Foundation; either version 3 of the License, or 
//'(at your option) any later version. 
//'Pachyderm-Acoustic is distributed in the hope that it will be useful, 
//'but WITHOUT ANY WARRANTY; without even the implied warranty of 
//'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
//'GNU General Public License for more details. 
//' 
//'You should have received a copy of the GNU General Public 
//'License along with Pachyderm-Acoustic; if not, write to the Free Software 
//'Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA. 

using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class SPLAETC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public SPLAETC()
            : base("A-weighted Sound Pressure Level", "SPL-A",
                "Computes Sound Pressure Level (A) from Energy Time Curve",
                "Acoustics", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Energy Time Curve", "ETC", "Energy Time Curve", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Sound Pressure Level(A)", "SPLA", "A-weighted Sound Pressure Level", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Audio_Signal> ETC = new List<Audio_Signal>();
            double SW = 0;
            List<double> spec = new List<double>();

            if (DA.GetDataList<double>(0, spec))
            {
                if (spec.Count != 8) throw new Exception("For A-Weighted Sound Pressure Level, full spectrum data is needed. Please supply data for octaves 0 through 7.");
                SW = Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(spec.ToArray());
                DA.SetData(0, SW);
            }
            else if (DA.GetDataList<Audio_Signal>(0, ETC))
            {
                for (int i = 0; i < ETC.Count; i++)
                {
                    if (ETC[i].ChannelCount != 8) throw new Exception("For A-Weighted Sound Pressure Level, full spectrum data is needed. Please supply data for octaves 0 through 7.");

                    double[] AFactors = new double[8] { Math.Pow(10, (-26.2 / 10)), Math.Pow(10, (-16.1 / 10)), Math.Pow(10, (-8.6 / 10)), Math.Pow(10, (-3.2 / 10)), 1, Math.Pow(10, (1.2 / 10)), Math.Pow(10, (1 / 10)), Math.Pow(10, (-1.1 / 10)) };

                    for (int f = 0; f < ETC[i].ChannelCount; f++)
                    {
                        double s = 0;
                        for (int j = 0; j < ETC.Count; j++) s += ETC[i][f][j];
                        SW += s * AFactors[f];
                        DA.SetData(0, Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(SW));
                    }
                }
            }
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.SPL;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{C33A1F42-9EEC-4D7F-AB99-13C5CF0812B2}"); }
        }
    }
}