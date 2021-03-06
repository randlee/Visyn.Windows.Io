#region Copyright (c) 2015-2018 Visyn
// The MIT License(MIT)
// 
// Copyright (c) 2015-2018 Visyn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System.Reflection;
using System.Text;
using Visyn.Exceptions;
using Visyn.Mathematics;
using Visyn.Serialize;
using Visyn.Windows.Io.FileHelper.Enums;
using Visyn.Windows.Io.FileHelper.Attributes;

namespace Visyn.Windows.Io.FileHelper.Fields
{
    /// <summary>
    /// Fixed length field that has length and alignment
    /// </summary>
    public sealed class FixedLengthField : FieldBase
    {
        #region "  Properties  "

        /// <summary>
        /// Field length of this field in the record
        /// </summary>
        internal int FieldLength { get; private set; }

        /// <summary>
        /// Alignment of this record
        /// </summary>
        internal FieldAlignAttribute Align { get; private set; }

        /// <summary>
        /// Whether we allow more or less characters to be handled
        /// </summary>
        internal FixedMode FixedMode { get; set; }

        #endregion

        #region "  Constructor  "

        /// <summary>
        /// Simple fixed length field constructor
        /// </summary>
        private FixedLengthField() {}

        /// <summary>
        /// Create a fixed length field from field information
        /// </summary>
        /// <param name="fi">Field definitions</param>
        /// <param name="length">Length of this field</param>
        /// <param name="align">Alignment, left or right</param>
        internal FixedLengthField(FieldInfo fi, int length, FieldAlignAttribute align)
            : base(fi,null)
        {
            FixedMode = FixedMode.ExactLength;
            Align = new FieldAlignAttribute(AlignMode.Left, ' ');
            FieldLength = length;

            if (align != null) Align = align;
            else if (fi.FieldType.IsNumeric()) Align = new FieldAlignAttribute(AlignMode.Right, ' ');
            //else if (fi.FieldType.IsNumeric()) Align = new FieldAlignAttribute(AlignMode.Right, ' ');

        }

        #endregion

        #region "  Overrides String Handling  "

        /// <summary>
        /// Get the value from the record
        /// </summary>
        /// <param name="line">line to extract from</param>
        /// <returns>Information extracted from record</returns>
        public override Visyn.Windows.Io.FileHelper.Core.ExtractedInfo ExtractFieldString(LineInfo line)
        {
            if (line.CurrentLength == 0)
            {
                if (IsOptional)
                    return Visyn.Windows.Io.FileHelper.Core.ExtractedInfo.Empty;
                throw new BadUsageException("End Of Line found processing the field: " + FieldInfo.Name +
                                            " at line " + line.mReader.LineNumber
                                            +
                                            ". (You need to mark it as [FieldOptional] if you want to avoid this exception)");
            }

            //ExtractedInfo res;

            if (line.CurrentLength < FieldLength)
            {
                if (FixedMode == FixedMode.AllowLessChars || FixedMode == FixedMode.AllowVariableLength)
                    return new Visyn.Windows.Io.FileHelper.Core.ExtractedInfo(line);
                throw new BadUsageException("The string '" + line.CurrentString + "' (length " +
                                            line.CurrentLength + ") at line "
                                            + line.mReader.LineNumber +
                                            " has less chars than the defined for " + FieldInfo.Name
                                            + " (" + FieldLength +
                                            "). You can use the [FixedLengthRecord(FixedMode.AllowLessChars)] to avoid this problem.");
            }
            if (line.CurrentLength > FieldLength &&
                IsArray == false &&
                IsLast &&
                FixedMode != FixedMode.AllowMoreChars &&
                FixedMode != FixedMode.AllowVariableLength) {
                throw new BadUsageException("The string '" + line.CurrentString + "' (length " +
                                            line.CurrentLength + ") at line "
                                            + line.mReader.LineNumber +
                                            " has more chars than the defined for the last field "
                                            + FieldInfo.Name + " (" + FieldLength +
                                            ").You can use the [FixedLengthRecord(FixedMode.AllowMoreChars)] to avoid this problem.");
            }
            return new Visyn.Windows.Io.FileHelper.Core.ExtractedInfo(line, line.mCurrentPos + FieldLength);
        }

        /// <summary>
        /// Create a fixed length string representation (pad it out or truncate it)
        /// </summary>
        /// <param name="sb">buffer to add field to</param>
        /// <param name="fieldValue">value we are updating with</param>
        /// <param name="isLast">Indicates if we are processing last field</param>
        public override void CreateFieldString(StringBuilder sb, object fieldValue, bool isLast)
        {
            var field = base.CreateFieldString(fieldValue);

            // Discard longer field values
            if (field.Length > FieldLength)
                field = field.Substring(0, FieldLength);

            switch (Align.Align)
            {
                case AlignMode.Left:
                    sb.Append(field);
                    sb.Append(Align.AlignChar, FieldLength - field.Length);
                    break;
                case AlignMode.Right:
                    sb.Append(Align.AlignChar, FieldLength - field.Length);
                    sb.Append(field);
                    break;
                case AlignMode.Center:
                default:
                    var middle = (FieldLength - field.Length)/2;

                    sb.Append(Align.AlignChar, middle);
                    sb.Append(field);
                    sb.Append(Align.AlignChar, FieldLength - field.Length - middle);
                    break;
            }
        }

        /// <summary>
        /// Create a clone of the fixed length record ready to get updated by
        /// the base settings
        /// </summary>
        /// <returns>new fixed length field definition just like this one minus
        /// the base settings</returns>
        protected override FieldBase CreateClone()
        {
            var res = new FixedLengthField {
                Align = Align,
                FieldLength = FieldLength,
                FixedMode = FixedMode
            };
            return res;
        }

        #endregion
    }
}
