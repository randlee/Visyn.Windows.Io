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

namespace Visyn.Windows.Io.FileHelper.Messages
{
  

    public class ErrorMessages
    {
        public class ClassWithOutDefaultConstructorClass : MessageBase
        {
            internal ClassWithOutDefaultConstructorClass() : base(@"The record class $ClassName$ needs a constructor with no args (public or private)") { }
            private string mClassName = null;
            public ClassWithOutDefaultConstructorClass ClassName(string value)
            {
                mClassName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$ClassName$", mClassName);
                return res;
            }
        }
        public class ClassWithOutFieldsClass : MessageBase
        {

            internal ClassWithOutFieldsClass() : base(@"The record class $ClassName$ don't contains any field") { }
            private string mClassName = null;
            public ClassWithOutFieldsClass ClassName(string value)
            {
                mClassName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$ClassName$", mClassName);
                return res;
            }


        }
        public class ClassWithOutRecordAttributeClass : MessageBase
        {

            internal ClassWithOutRecordAttributeClass() : base(@"The record class $ClassName$ must be marked with the [DelimitedRecord] or [FixedLengthRecord] Attribute") { }
            private string mClassName = null;
            public ClassWithOutRecordAttributeClass ClassName(string value)
            {
                mClassName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$ClassName$", mClassName);
                return res;
            }


        }
        public class EmptyClassNameClass : MessageBase
        {

            internal EmptyClassNameClass() : base(@"The ClassName can't be empty") { }
            protected override string GenerateText()
            {
                var res = SourceText;
                return res;
            }


        }
        public class EmptyFieldNameClass : MessageBase
        {

            internal EmptyFieldNameClass() : base(@"The $Position$th field name can't be empty") { }
            private string mPosition = null;
            public EmptyFieldNameClass Position(string value)
            {
                mPosition = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$Position$", mPosition);
                return res;
            }


        }
        public class EmptyFieldTypeClass : MessageBase
        {

            internal EmptyFieldTypeClass() : base(@"The $Position$th field type can't be empty") { }
            private string mPosition = null;
            public EmptyFieldTypeClass Position(string value)
            {
                mPosition = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$Position$", mPosition);
                return res;
            }


        }
        public class ExpectingFieldOptionalClass : MessageBase
        {

            internal ExpectingFieldOptionalClass() : base(@"The field: $FieldName$ must be marked as optional because the previous field is marked as optional. (Try adding [FieldOptional] to $FieldName$)") { }
            private string mFieldName = null;
            public ExpectingFieldOptionalClass FieldName(string value)
            {
                mFieldName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName$", mFieldName);
                return res;
            }


        }
        public class FieldNotFoundClass : MessageBase
        {

            internal FieldNotFoundClass() : base(@"The field: $FieldName$ was not found in the class: $ClassName$. Remember that this option is case sensitive") { }
            private string mFieldName = null;
            public FieldNotFoundClass FieldName(string value)
            {
                mFieldName = value;
                return this;
            }
            private string mClassName = null;
            public FieldNotFoundClass ClassName(string value)
            {
                mClassName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName$", mFieldName);
                res = StringHelper.ReplaceIgnoringCase(res, "$ClassName$", mClassName);
                return res;
            }


        }
        public class FieldOptionalClass : MessageBase
        {

            internal FieldOptionalClass() : base(@"The field: $Field$ must be marked as optional because the previous field is marked with FieldOptional. (Try adding [FieldOptional] to $Field$)") { }
            private string mField = null;
            public FieldOptionalClass Field(string value)
            {
                mField = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$Field$", mField);
                return res;
            }


        }
        public class InvalidIdentifierClass : MessageBase
        {

            internal InvalidIdentifierClass() : base(@"The string '$Identifier$' not is a valid .NET identifier") { }
            private string mIdentifier = null;
            public InvalidIdentifierClass Identifier(string value)
            {
                mIdentifier = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$Identifier$", mIdentifier);
                return res;
            }


        }
        public class MissingFieldArrayLenghtInNotLastFieldClass : MessageBase
        {

            internal MissingFieldArrayLenghtInNotLastFieldClass() : base(@"The field: $FieldName$ is of an array type and must contain a [FieldArrayLength] attribute because it is not the last field") { }
            private string mFieldName = null;
            public MissingFieldArrayLenghtInNotLastFieldClass FieldName(string value)
            {
                mFieldName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName$", mFieldName);
                return res;
            }


        }
        public class MixOfStandardAndAutoPropertiesFieldsClass : MessageBase
        {

            internal MixOfStandardAndAutoPropertiesFieldsClass() : base(@"You can mix standard fields and automatic properties only if you use [FieldOrder()] over the fields and properties in the $ClassName$ class.") { }
            private string mClassName = null;
            public MixOfStandardAndAutoPropertiesFieldsClass ClassName(string value)
            {
                mClassName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$ClassName$", mClassName);
                return res;
            }


        }
        public class NullRecordClassClass : MessageBase
        {

            internal NullRecordClassClass() : base(@"The record type can't be null") { }
            protected override string GenerateText()
            {
                var res = SourceText;
                return res;
            }


        }
        public class PartialFieldOrderClass : MessageBase
        {

            internal PartialFieldOrderClass() : base(@"The field: $FieldName$ must be marked with FieldOrder because if you use this attribute in one field you must also use it on all of them.") { }
            private string mFieldName = null;
            public PartialFieldOrderClass FieldName(string value)
            {
                mFieldName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName$", mFieldName);
                return res;
            }


        }
        public class PartialFieldOrderInAutoPropertyClass : MessageBase
        {

            internal PartialFieldOrderInAutoPropertyClass() : base(@"The auto property: $PropertyName$ must be marked with FieldOrder because if you use this attribute in one field you must also use it on all of them.") { }
            private string mPropertyName = null;
            public PartialFieldOrderInAutoPropertyClass PropertyName(string value)
            {
                mPropertyName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$PropertyName$", mPropertyName);
                return res;
            }


        }
        public class SameFieldOrderClass : MessageBase
        {

            internal SameFieldOrderClass() : base(@"The field: $FieldName1$ has the same FieldOrder that: $FieldName2$ you must use different values") { }
            private string mFieldName1 = null;
            public SameFieldOrderClass FieldName1(string value)
            {
                mFieldName1 = value;
                return this;
            }
            private string mFieldName2 = null;
            public SameFieldOrderClass FieldName2(string value)
            {
                mFieldName2 = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName1$", mFieldName1);
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName2$", mFieldName2);
                return res;
            }


        }
        public class SameMinMaxLengthForArrayNotLastFieldClass : MessageBase
        {

            internal SameMinMaxLengthForArrayNotLastFieldClass() : base(@"The array field: $FieldName$ must be of a fixed length because it is not the last field of the class, i.e. the min and max length of the [FieldArrayLength] attribute must be the same.") { }
            private string mFieldName = null;
            public SameMinMaxLengthForArrayNotLastFieldClass FieldName(string value)
            {
                mFieldName = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName$", mFieldName);
                return res;
            }


        }
        public class StructRecordClassClass : MessageBase
        {

            internal StructRecordClassClass() : base(@"The record type must be a class, and the type: $RecordType$ is a struct.") { }
            private string mRecordType = null;
            public StructRecordClassClass RecordType(string value)
            {
                mRecordType = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$RecordType$", mRecordType);
                return res;
            }


        }
        public class TestQuoteClass : MessageBase
        {

            internal TestQuoteClass() : base(@"The Message class also allows to use "" in any part of the "" text "" .") { }
            protected override string GenerateText()
            {
                var res = SourceText;
                return res;
            }


        }
        public class WrongConverterClass : MessageBase
        {

            internal WrongConverterClass() : base(@"The converter for the field: $FieldName$ returns an object of Type: $ConverterReturnedType$  and the field is of type: $FieldType$") { }
            private string mFieldName = null;
            public WrongConverterClass FieldName(string value)
            {
                mFieldName = value;
                return this;
            }
            private string mConverterReturnedType = null;
            public WrongConverterClass ConverterReturnedType(string value)
            {
                mConverterReturnedType = value;
                return this;
            }
            private string mFieldType = null;
            public WrongConverterClass FieldType(string value)
            {
                mFieldType = value;
                return this;
            }
            protected override string GenerateText()
            {
                var res = SourceText;
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldName$", mFieldName);
                res = StringHelper.ReplaceIgnoringCase(res, "$ConverterReturnedType$", mConverterReturnedType);
                res = StringHelper.ReplaceIgnoringCase(res, "$FieldType$", mFieldType);
                return res;
            }
        }
    }
}
