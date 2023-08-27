﻿//
// Author:
//   Michael Göricke
//
// Copyright (c) 2023
//
// This file is part of FocalMaster.
//
// ShapeConverter is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Windows.Media;
using PdfSharp.Pdf;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Shading
{
    /// <summary>
    /// The shading descriptor interface
    /// </summary>
    internal interface IShading
    {
        /// <summary>
        /// Init
        /// </summary>
        void Init(PdfDictionary shadingDict);

        /// <summary>
        /// Get a brush
        /// </summary>
        GraphicBrush GetBrush(Matrix matrix, PdfRect boundingBox, double alpha, List<FunctionStop> softMask);
    }
}
