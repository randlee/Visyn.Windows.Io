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

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Visyn.Exceptions;
using Visyn.Serialize;
using Visyn.Serialize.Converters;
using Visyn.Types;
using Visyn.Windows.Io.FileHelper.Attributes;
using Visyn.Windows.Io.FileHelper.Enums;
using Visyn.Windows.Io.FileHelper.Options;

namespace Visyn.Windows.Io.FileHelper.Fields
{
    /// <summary>
    /// Base class for all Field Types.
    /// Implements all the basic functionality of a field in a typed file.
    /// </summary>
    public abstract class FieldBase : ICloneable
    {
        #region "  Private & Internal Fields  "

        // --------------------------------------------------------------
        // WARNING !!!
        //    Remember to add each of these fields to the clone method !!
        // --------------------------------------------------------------

        /// <summary>
        /// type of object to be created,  eg DateTime
        /// </summary>
        public Type FieldType { get; private set; }

        /// <summary>
        /// Provider to convert to and from text
        /// </summary>
        public IFieldConverter Converter { get; private set; }

        /// <summary>
        /// Number of extra characters used,  delimiters and quote characters
        /// </summary>
        public virtual int CharsToDiscard => 0;

        /// <summary>
        /// Field type of an array or it is just fieldType.
        /// What actual object will be created
        /// </summary>
        public Type FieldTypeInternal { get; set; }

        /// <summary>
        /// Is this field an array?
        /// </summary>
        public bool IsArray { get; private set; }

        /// <summary>
        /// Array must have this many entries
        /// </summary>
        public int ArrayMinLength { get; set; }

        /// <summary>
        /// Array may have this many entries,  if equal to ArrayMinLength then
        /// it is a fixed length array
        /// </summary>
        public int ArrayMaxLength { get; set; }

        /// <summary>
        /// Seems to be duplicate of FieldTypeInternal except it is ONLY set
        /// for an array
        /// </summary>
        public Type ArrayType { get; set; }

        /// <summary>
        /// Do we process this field but not store the value
        /// </summary>
        public bool Discarded { get; set; }

        /// <summary>
        /// Unused!
        /// </summary>
        public bool TrailingArray { get; set; }

        /// <summary>
        /// Value to use if input is null or empty
        /// </summary>
        public object NullValue { get; set; }

        /// <summary>
        /// Are we a simple string field we can just assign to
        /// </summary>
        public bool IsStringField { get; set; }

        /// <summary>
        /// Details about the extraction criteria
        /// </summary>
        public FieldInfo FieldInfo { get; set; }

        /// <summary>
        /// indicates whether we trim leading and/or trailing whitespace
        /// </summary>
        public TrimMode TrimMode { get; set; }

        /// <summary>
        /// Character to chop off front and / rear of the string
        /// </summary>
        public char[] TrimChars { get; set; }

        /// <summary>
        /// The field may not be present on the input data (line not long enough)
        /// </summary>
        public bool IsOptional
        {
            get; set;
        }

        /// <summary>
        /// The next field along is optional,  optimise processing next records
        /// </summary>
        public bool NextIsOptional => Parent.FieldCount > ParentIndex + 1 && Parent.Fields[ParentIndex + 1].IsOptional;


        /// <summary>
        /// Am I the first field in an array list
        /// </summary>
        public bool IsFirst => ParentIndex == 0;

        /// <summary>
        /// Am I the last field in the array list
        /// </summary>
        public bool IsLast => ParentIndex == Parent.FieldCount - 1;


        /// <summary>
        /// Set from the FieldInNewLIneAtribute.  This field begins on a new
        /// line of the file
        /// </summary>
        public bool InNewLine { get; set; }

        /// <summary>
        /// Order of the field in the file layout
        /// </summary>
        public int? FieldOrder { get; set; }

        /// <summary>
        /// Can null be assigned to this value type, for example not int or
        /// DateTime
        /// </summary>
        public bool IsNullableType { get; private set; }

        /// <summary>
        /// Name of the field without extra characters (eg property)
        /// </summary>
        public string FieldFriendlyName { get; set; }

        /// <summary>
        /// The field must be not be empty
        /// </summary>
        public bool IsNotEmpty { get; set; }

        /// <summary>
        /// Caption of the field displayed in header row (see EngineBase.GetFileHeader)
        /// </summary>
        public string FieldCaption { get; set; }

        // --------------------------------------------------------------
        // WARNING !!!
        //    Remember to add each of these fields to the clone method !!
        // --------------------------------------------------------------

        /// <summary>
        /// Fieldname of the field we are storing
        /// </summary>
        public string FieldName => FieldInfo.Name;

        #endregion

        #region "  CreateField  "

        /// <summary>
        /// Check the Attributes on the field and return a structure containing
        /// the settings for this file.
        /// </summary>
        /// <param name="fi">Information about this field</param>
        /// <param name="recordAttribute">Type of record we are reading</param>
        /// <returns>Null if not used</returns>
        public static FieldBase CreateField(FieldInfo fi, ITypedRecordAttribute recordAttribute)
        {
            FieldBase res = null;
            MemberInfo mi = fi;
            var memberName = $"The field: '{ fi.Name}'";
            var fieldType = fi.FieldType;             
            var fieldFriendlyName = AutoPropertyName(fi);
            if (string.IsNullOrEmpty(fieldFriendlyName)==false)
            {                
                var prop = fi.DeclaringType.GetProperty(fieldFriendlyName);
                if (prop != null)
                {
                    memberName = $"The property: '{ prop.Name}'";
                    mi = prop;
                }
                else
                {
                    fieldFriendlyName = null;
                }
            }
            // If ignored, return null
#pragma warning disable 612,618 // disable obsolete warning
            if (mi.IsDefined(typeof (FieldNotInFileAttribute), true) ||
                mi.IsDefined(typeof (FieldIgnoredAttribute), true) ||
                mi.IsDefined(typeof (FieldHiddenAttribute), true))
#pragma warning restore 612,618
                return null;

            var attributes = (FieldAttribute[]) mi.GetCustomAttributes(typeof (FieldAttribute), true);

            // CHECK USAGE ERRORS !!!

            // Fixed length record and no attributes at all
            if (recordAttribute is FixedLengthRecordAttribute && attributes.Length == 0)
            {
                throw new BadUsageException($"{memberName} must be marked the FieldFixedLength attribute because the record class is marked with FixedLengthRecord.");
            }

            if (attributes.Length > 1)
            {
                throw new BadUsageException($"{memberName} has a FieldFixedLength and a FieldDelimiter attribute.");
            }

            if (recordAttribute is DelimitedRecordAttribute && mi.IsDefined(typeof (FieldAlignAttribute), false))
            {
                throw new BadUsageException($"{memberName} can't be marked with FieldAlign attribute, it is only valid for fixed length records and are used only for write purpose.");
            }

            if (fieldType.IsArray == false && mi.IsDefined(typeof (FieldArrayLengthAttribute), false))
            {
                throw new BadUsageException($"{memberName} can't be marked with FieldArrayLength attribute is only valid for array fields.");
            }

            // PROCESS IN NORMAL CONDITIONS
            if (attributes.Length > 0)
            {
                var fieldAttribute = attributes[0];

                var fixedLengthAttribute = fieldAttribute as FieldFixedLengthAttribute;
                if (fixedLengthAttribute != null)
                {
                    // Fixed Field
                    if (recordAttribute is DelimitedRecordAttribute)
                    {
                        throw new BadUsageException($"{memberName} can't be marked with FieldFixedLength attribute, it is only for the FixedLengthRecords not for delimited ones.");
                    }

                    var alignAttribute = mi.GetFirst<FieldAlignAttribute>();

                    res = new FixedLengthField(fi, fixedLengthAttribute.Length, alignAttribute);
                    ((FixedLengthField) res).FixedMode = ((FixedLengthRecordAttribute) recordAttribute).FixedMode;
                }
                else if (fieldAttribute is FieldDelimiterAttribute)
                {
                    // Delimited Field
                    if (recordAttribute is FixedLengthRecordAttribute)
                    {
                        throw new BadUsageException($"{memberName} can't be marked with FieldDelimiter attribute, it is only for DelimitedRecords not for fixed ones.");
                    }
                    res = new DelimitedField(fi, ((FieldDelimiterAttribute) fieldAttribute).Delimiter);
                }
                else
                {
                    throw new BadUsageException(
                        $"Custom field attributes are not currently supported. Unknown attribute: {fieldAttribute.GetType().Name} on field: { fi.Name}");
                }
            }
            else // attributes.Length == 0
            {
                var delimitedRecordAttribute = recordAttribute as DelimitedRecordAttribute;

                if (delimitedRecordAttribute != null)
                {
                    res = new DelimitedField(fi, delimitedRecordAttribute.Delimiter);
                }
            }

            if (res != null)
            {
                // FieldDiscarded
                res.Discarded = mi.IsDefined(typeof (FieldValueDiscardedAttribute), false);

                // FieldTrim
                mi.WorkWithFirst<FieldTrimAttribute>((x) => {
                        res.TrimMode = x.TrimMode;
                        res.TrimChars = x.TrimChars;
                    });

                // FieldQuoted
                mi.WorkWithFirst<FieldQuotedAttribute>((x) => {
                        if (res is FixedLengthField)  throw new BadUsageException( $"{memberName} can't be marked with FieldQuoted attribute, it is only for the delimited records.");

                        ((DelimitedField) res).QuoteChar = x.QuoteChar;
                        ((DelimitedField) res).QuoteMode = x.QuoteMode;
                        ((DelimitedField) res).QuoteMultiline = x.QuoteMultiline;
                    });

                // FieldOrder
                mi.WorkWithFirst<FieldOrderAttribute>(x => res.FieldOrder = x.Order);

                // FieldCaption
                mi.WorkWithFirst<FieldCaptionAttribute>(x => res.FieldCaption = x.Caption);

                // FieldOptional
                res.IsOptional = mi.IsDefined(typeof(FieldOptionalAttribute), false);

                // FieldInNewLine
                res.InNewLine = mi.IsDefined(typeof(FieldInNewLineAttribute), false);

                // FieldNotEmpty
                res.IsNotEmpty = mi.IsDefined(typeof(FieldNotEmptyAttribute), false);

                // FieldArrayLength
                if (fieldType.IsArray)
                {
                    res.IsArray = true;
                    res.ArrayType = fieldType.GetElementType();

                    // MinValue indicates that there is no FieldArrayLength in the array
                    res.ArrayMinLength = int.MinValue;
                    res.ArrayMaxLength = int.MaxValue;

                    mi.WorkWithFirst<FieldArrayLengthAttribute>((x) => {
                        res.ArrayMinLength = x.MinLength;
                        res.ArrayMaxLength = x.MaxLength;

                        if (res.ArrayMaxLength < res.ArrayMinLength || res.ArrayMinLength < 0 || res.ArrayMaxLength <= 0)
                            throw new BadUsageException($"{memberName} has invalid length values in the [FieldArrayLength] attribute.");
                    });
                }
            }

            if (string.IsNullOrEmpty(res.FieldFriendlyName))
                res.FieldFriendlyName = res.FieldName;

            return res;
        }

        public RecordOptions Parent { get; set; }
        public int ParentIndex { get; set; }

        public static string AutoPropertyName(FieldInfo fi)
        {
            if (!fi.IsDefined(typeof(CompilerGeneratedAttribute), false)) return "";

            if (fi.Name.EndsWith("__BackingField") &&
                fi.Name.StartsWith("<") &&
                fi.Name.Contains(">"))
                return fi.Name.Substring(1, fi.Name.IndexOf(">") - 1);
            return "";
        }

        public bool IsAutoProperty { get; set; }

        #endregion

        #region "  Constructor  "

        /// <summary>
        /// Create a field base without any configuration
        /// </summary>
        protected FieldBase()
        {
            IsNullableType = false;
            TrimMode = TrimMode.None;
            FieldOrder = null;
            InNewLine = false;
            //NextIsOptional = false;
            IsOptional = false;
            TrimChars = null;
            NullValue = null;
            TrailingArray = false;
            IsArray = false;
            IsNotEmpty = false;
        }

        /// <summary>
        /// Create a field base from a fieldinfo object
        /// Verify the settings against the actual field to ensure it will work.
        /// </summary>
        /// <param name="fi">Field Info Object</param>
        protected FieldBase(FieldInfo fi, string delimiter)  : this()
        {
            FieldInfo = fi;
            FieldType = FieldInfo.FieldType;
            MemberInfo attibuteTarget = fi;
            FieldFriendlyName = AutoPropertyName(fi);
            if (string.IsNullOrEmpty(FieldFriendlyName) == false)
            {
                var prop = fi.DeclaringType.GetProperty(FieldFriendlyName);
                if (prop == null)
                {
                    FieldFriendlyName = null;
                }
                else
                {
                    IsAutoProperty = true;
                    attibuteTarget = prop;
                }
            }

            FieldTypeInternal = FieldType.IsArray ? FieldType.GetElementType() : FieldType;

            IsStringField = FieldTypeInternal == typeof (string);

            var attributes = attibuteTarget.GetCustomAttributes(typeof (FieldConverterAttribute), true);

            foreach (var attribute in attributes)
            {
                var fc = attribute as FieldConverterAttribute;
                if (fc != null) fc.Delimiter = delimiter;
#if DEBUG
                var del = attribute as DelimitedRecordAttribute;
                if (del != null) Debug.Assert(del.Delimiter == delimiter);
#endif
            }
            if (attributes.Length > 0) {
                var conv = (FieldConverterAttribute) attributes[0];
                Converter = conv.Converter;
                conv.ValidateTypes(FieldInfo);
            }
            else
                Converter = ConverterFactory.GetDefaultConverter(FieldFriendlyName ?? fi.Name, FieldType);

            if (Converter != null)
            {
                if(Converter.Type == null)
                    throw new ArgumentNullException(nameof(IFieldConverter.Type), $"Converter type [{Converter.GetType().Name}] can not be null!");
                if(Converter.Type != FieldTypeInternal)
                    throw new TypeMismatchException(Converter.Type, $"{Converter.GetType().Name} destination type {Converter.Type} != expected type {FieldTypeInternal}");
                //Converter.mDestinationType = FieldTypeInternal;
            }

            attributes = attibuteTarget.GetCustomAttributes(typeof (FieldNullValueAttribute), true);

            if (attributes.Length > 0) {
                NullValue = ((FieldNullValueAttribute) attributes[0]).NullValue;
                //				mNullValueOnWrite = ((FieldNullValueAttribute) attribs[0]).NullValueOnWrite;

                if (NullValue != null) {
                    if (!FieldTypeInternal.IsAssignableFrom(NullValue.GetType()))
                    {
                        throw new BadUsageException(
                            $"The NullValue is of type: {NullValue.GetType().Name} which is not asignable to the field {FieldInfo.Name} Type: {FieldTypeInternal.Name}");
                    }
                }
            }

            IsNullableType = FieldTypeInternal.IsValueType &&
                             FieldTypeInternal.IsGenericType &&
                             FieldTypeInternal.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        #endregion

        #region "  MustOverride (String Handling)  "

        /// <summary>
        /// Extract the string from the underlying data, removes quotes
        /// characters for example
        /// </summary>
        /// <param name="line">Line to parse data from</param>
        /// <returns>Slightly processed string from the data</returns>
        public abstract Visyn.Windows.Io.FileHelper.Core.ExtractedInfo ExtractFieldString(LineInfo line);

        /// <summary>
        /// Create a text block containing the field from definition
        /// </summary>
        /// <param name="sb">Append string to output</param>
        /// <param name="fieldValue">Field we are adding</param>
        /// <param name="isLast">Indicates if we are processing last field</param>
        public abstract void CreateFieldString(StringBuilder sb, object fieldValue, bool isLast);

        /// <summary>
        /// Convert a field value to a string representation
        /// </summary>
        /// <param name="fieldValue">Object containing data</param>
        /// <returns>String representation of field</returns>
        public string CreateFieldString(object fieldValue)
        {
            if (Converter != null) return Converter.FieldToString(fieldValue);
            return fieldValue?.ToString() ?? string.Empty;
        }

        #endregion

        #region "  ExtractValue  "

        /// <summary>
        /// Get the data out of the records
        /// </summary>
        /// <param name="line">Line handler containing text</param>
        /// <returns></returns>
        public object ExtractFieldValue(LineInfo line)
        {
            //-> extract only what I need

            if (InNewLine)
            {   // Any trailing characters, terminate
                if (line.EmptyFromPos() == false) 
                    throw new BadUsageException(line, $"Text '{line.CurrentString}' found before the new line of the field: {FieldInfo.Name} (this is not allowed when you use [FieldInNewLine])");

                line.ReLoad(line.mReader.ReadNextLine());

                if (line.mLineStr == null) 
                    throw new BadUsageException(line, $"End of stream found parsing the field {FieldInfo.Name}. Please check the class record.");
            }

            if (IsArray == false)
            {
                var info = ExtractFieldString(line);
                if (info.mCustomExtractedString == null)
                    line.mCurrentPos = info.ExtractedTo + 1;

                line.mCurrentPos += CharsToDiscard; //total;

                return Discarded ? GetDiscardedNullValue() : AssignFromString(info, line).Value;
            }
            if (ArrayMinLength <= 0) ArrayMinLength = 0;

            var i = 0;

            var res = new ArrayList(Math.Max(ArrayMinLength, 10));

            while (line.mCurrentPos - CharsToDiscard < line.mLineStr.Length && i < ArrayMaxLength)
            {
                var info = ExtractFieldString(line);
                if (info.mCustomExtractedString == null)
                    line.mCurrentPos = info.ExtractedTo + 1;

                line.mCurrentPos += CharsToDiscard;

                try
                {
                    var value = AssignFromString(info, line);

                    if (value.NullValueUsed && i == 0 && line.IsEOL())
                        break;

                    res.Add(value.Value);
                }
                catch (NullValueNotFoundException)
                {
                    if (i == 0)
                        break;
                    throw;
                }
                i++;
            }

            if (res.Count < ArrayMinLength) 
                throw new InvalidOperationException( $"Line: {line.mReader.LineNumber} Column: {line.mCurrentPos} Field: {FieldInfo.Name}. The array has only {res.Count} values, less than the minimum length of {ArrayMinLength}");

            if (IsLast && line.IsEOL() == false) 
                throw new InvalidOperationException( $"Line: {line.mReader.LineNumber} Column: {line.mCurrentPos} Field: {FieldInfo.Name}. The array has more values than the maximum length of {ArrayMaxLength}");
            
            // TODO:   is there a reason we go through all the array processing then discard it
            return Discarded ? null : res.ToArray(ArrayType);
        }

        #region "  AssignFromString  "

        private struct AssignResult
        {
            public object Value;
            public bool NullValueUsed;
        }

        /// <summary>
        /// Create field object after extracting the string from the underlying
        /// input data
        /// </summary>
        /// <param name="fieldString">Information extracted?</param>
        /// <param name="line">Underlying input data</param>
        /// <returns>Object to assign to field</returns>
        private AssignResult AssignFromString(Visyn.Windows.Io.FileHelper.Core.ExtractedInfo fieldString, LineInfo line)
        {
            var extractedString = fieldString.ExtractedString();

            try {
                object val;
                if (IsNotEmpty && string.IsNullOrEmpty(extractedString)) {
                    throw new InvalidOperationException("The value is empty and must be populated.");
                }
                if (Converter == null)
                {
                    if (IsStringField)
                        val = TrimString(extractedString);
                    else
                    {
                        extractedString = extractedString.Trim();

                        if (extractedString.Length == 0)
                        {
                            return new AssignResult{ Value = GetNullValue(line), NullValueUsed = true };
                        }
                        val = Convert.ChangeType(extractedString, FieldTypeInternal, null);
                    }
                }
                else
                {
                    var trimmedString = extractedString.Trim();

                    if (Converter.CustomNullHandling == false && trimmedString.Length == 0)
                    {
                        return new AssignResult{ Value = GetNullValue(line), NullValueUsed = true };
                    }
                    val = Converter.StringToField(TrimMode == TrimMode.Both ? trimmedString : TrimString(extractedString));

                    if (val == null) {
                        return new AssignResult {  Value = GetNullValue(line), NullValueUsed = true };
                    }
                }
                return new AssignResult {  Value = val };
            }
            catch (ConvertException ex)
            {
                ex.FieldName = FieldInfo.Name;
                ex.LineNumber = line.mReader.LineNumber;
                ex.ColumnNumber = fieldString.ExtractedFrom + 1;
                throw;
            }
            catch (BadUsageException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (Converter == null || Converter.GetType().Assembly == typeof (FieldBase).Assembly)
                {
                    throw new ConvertException(extractedString, FieldTypeInternal, FieldInfo.Name, line.mReader.LineNumber, fieldString.ExtractedFrom + 1,ex.Message, ex);
                }
                throw new ConvertException(extractedString, FieldTypeInternal, FieldInfo.Name,
                    line.mReader.LineNumber, fieldString.ExtractedFrom + 1,
                    $"Your custom converter: { Converter.GetType().Name} throws an {ex.GetType().Name} with the message: {ex.Message}",ex);
            }
        }

        private string TrimString(string extractedString)
        {
            switch (TrimMode) {
                case TrimMode.None: return extractedString;
                case TrimMode.Both:  return extractedString.Trim();
                case TrimMode.Left: return extractedString.TrimStart();
                case TrimMode.Right: return extractedString.TrimEnd();
                default:
                    throw new Exception($"Trim mode invalid in FieldBase.TrimString -> {TrimMode}");
            }
        }

        /// <summary>
        /// Convert a null value into a representation,
        /// allows for a null value override
        /// </summary>
        /// <param name="line">input line to read, used for error messages</param>
        /// <returns>Null value for object</returns>
        private object GetNullValue(LineInfo line)
        {
            if (NullValue != null) return NullValue;
            if (!FieldTypeInternal.IsValueType) return null;
            if (IsNullableType) return null;

            var msg = "Not value found for the value type field: '" + FieldInfo.Name + "' Class: '" + FieldInfo.DeclaringType.Name + "'. " + Environment.NewLine +
                      "You must use the [FieldNullValue] attribute because this is a value type and can't be null or use a Nullable Type instead of the current type.";

            throw new NullValueNotFoundException(line, msg);
        }

        /// <summary>
        /// Get the null value that represent a discarded value
        /// </summary>
        /// <returns>null value of discard?</returns>
        private object GetDiscardedNullValue()
        {
            if (NullValue != null) return NullValue;
            if (!FieldTypeInternal.IsValueType) return null;
            if (IsNullableType) return null;

            var msg = "The field: '" + FieldInfo.Name + "' Class: '" + FieldInfo.DeclaringType?.Name +
                      "' is from a value type: " + FieldInfo.FieldType.Name +
                      " and is discarded (null) you must provide a [FieldNullValue] attribute.";

            throw new BadUsageException(msg);
        }

        #endregion

        #region "  CreateValueForField  "

        /// <summary>
        /// Convert a field value into a write able value
        /// </summary>
        /// <param name="fieldValue">object value to convert</param>
        /// <returns>converted value</returns>
        public object CreateValueForField(object fieldValue)
        {
            object val = null;

            if (fieldValue == null)
            {
                if (NullValue == null)
                {
                    if (FieldTypeInternal.IsValueType && Nullable.GetUnderlyingType(FieldTypeInternal) == null)
                    {
                        throw new BadUsageException( $"Null Value found. You must specify a FieldNullValueAttribute in the {FieldInfo.Name} field of type {FieldTypeInternal.Name}, because this is a ValueType.");
                    }
                    val = null;
                }
                else
                    val = NullValue;
            }
            else if (FieldTypeInternal == fieldValue.GetType())
                val = fieldValue;
            else
            {
                if (Converter == null)
                    val = Convert.ChangeType(fieldValue, FieldTypeInternal, null);
                else
                {
                    try
                    {
                        if (Nullable.GetUnderlyingType(FieldTypeInternal) != null &&
                            Nullable.GetUnderlyingType(FieldTypeInternal) == fieldValue.GetType())
                            val = fieldValue;
                        else
                            val = Convert.ChangeType(fieldValue, FieldTypeInternal, null);
                    }
                    catch
                    {
                        val = Converter.StringToField(fieldValue.ToString());
                    }
                }
            }

            return val;
        }

        #endregion

        #endregion

        #region "  AssignToString  "

        /// <summary>
        /// convert field to string value and assign to a string builder
        /// buffer for output
        /// </summary>
        /// <param name="sb">buffer to collect record</param>
        /// <param name="fieldValue">value to convert</param>
        public void AssignToString(StringBuilder sb, object fieldValue)
        {
            if (InNewLine == true)
                sb.Append(Environment.NewLine);

            if (IsArray)
            {
                if (fieldValue == null)
                {
                    if (0 < ArrayMinLength)
                        throw new InvalidOperationException( $"Field: {FieldInfo.Name}. The array is null, but the minimum length is {ArrayMinLength}");
                    return;
                }

                var array = (IList) fieldValue;

                if (array.Count < ArrayMinLength)
                    throw new InvalidOperationException( $"Field: {FieldInfo.Name}. The array has {array.Count} values, but the minimum length is {ArrayMinLength}");

                if (array.Count > ArrayMaxLength)
                    throw new InvalidOperationException( $"Field: {FieldInfo.Name}. The array has {array.Count} values, but the maximum length is {ArrayMaxLength}");
                

                for (var i = 0; i < array.Count; i++)
                {
                    var val = array[i];
                    CreateFieldString(sb, val, IsLast && i == array.Count - 1);
                }
            }
            else
                CreateFieldString(sb, fieldValue, IsLast);
        }

        #endregion

        /// <summary>
        /// Copy the field object
        /// </summary>
        /// <returns>a complete copy of the Field object</returns>
        object ICloneable.Clone()
        {
            var res = CreateClone();

            res.FieldType = FieldType;
            res.Converter = Converter;
            res.FieldTypeInternal = FieldTypeInternal;
            res.IsArray = IsArray;
            res.ArrayType = ArrayType;
            res.ArrayMinLength = ArrayMinLength;
            res.ArrayMaxLength = ArrayMaxLength;
            res.TrailingArray = TrailingArray;
            res.NullValue = NullValue;
            res.IsStringField = IsStringField;
            res.FieldInfo = FieldInfo;
            res.TrimMode = TrimMode;
            res.TrimChars = TrimChars;
            res.IsOptional = IsOptional;
            //res.NextIsOptional = NextIsOptional;
            res.InNewLine = InNewLine;
            res.FieldOrder = FieldOrder;
            res.IsNullableType = IsNullableType;
            res.Discarded = Discarded;
            res.FieldFriendlyName = FieldFriendlyName;
            res.IsNotEmpty = IsNotEmpty;
            res.FieldCaption = FieldCaption;
            res.Parent = Parent;
            res.ParentIndex = ParentIndex;
            return res;
        }

        /// <summary>
        /// Add the extra details that derived classes create
        /// </summary>
        /// <returns>field clone of right type</returns>
        protected abstract FieldBase CreateClone();
    }
}
