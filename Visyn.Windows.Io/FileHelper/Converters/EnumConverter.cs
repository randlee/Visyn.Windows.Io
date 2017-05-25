using System;
using Visyn.Windows.Io.Exceptions;

namespace Visyn.Windows.Io.FileHelper.Converters
{
    internal sealed class EnumConverter : ConverterBase
    {
        private readonly Type mEnumType;

        public EnumConverter(Type sourceEnum)
        {
            if (sourceEnum.IsEnum == false)
                throw new BadUsageException("The Input sourceType must be an Enum but is of type " + sourceEnum.Name);

            mEnumType = sourceEnum;
        }

        public override object StringToField(string from)
        {
            try {
                return Enum.Parse(mEnumType, from.Trim(), true);
            }
            catch (ArgumentException) {
                throw new ConvertException(from, mEnumType, $"The value {from} is not present in the Enum.");
            }
        }
    }
}