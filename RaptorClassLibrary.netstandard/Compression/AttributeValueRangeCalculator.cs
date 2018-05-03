namespace VSS.VisionLink.Raptor.Compression
{
    public static class AttributeValueRangeCalculator
    {
        // CalculateAttributeValueRange scans a single attribute across all records in a block of values
        public static void CalculateAttributeValueRange(int[] Values,
                                                        uint Mask,
                                                        int ANativeNullValue, bool ANullable,
                                                        ref EncodedBitFieldDescriptor FieldDescriptor)
        {
            bool ObservedANullValue = false;
            bool FirstValue = true;

            FieldDescriptor.NativeNullValue = ANativeNullValue;
            FieldDescriptor.MinValue = ANativeNullValue;
            FieldDescriptor.MaxValue = ANativeNullValue;
            FieldDescriptor.Nullable = ANullable;

            foreach (var t in Values)
            {
                int TestValue = t;

                // Ensure negative values are preserved
                TestValue = (TestValue < 0) ? (int)(TestValue & Mask) | 0x8000000 : (int)(TestValue & Mask);

                if (FieldDescriptor.Nullable)
                {
                    if (FieldDescriptor.MinValue == ANativeNullValue || (TestValue != ANativeNullValue && TestValue < FieldDescriptor.MinValue))
                    {
                        FieldDescriptor.MinValue = TestValue;
                    }

                    if (FieldDescriptor.MaxValue == ANativeNullValue || (TestValue != ANativeNullValue && TestValue > FieldDescriptor.MaxValue))
                    {
                        FieldDescriptor.MaxValue = TestValue;
                    }
                }
                else
                {
                    if (FirstValue || TestValue < FieldDescriptor.MinValue)
                    {
                        FieldDescriptor.MinValue = TestValue;
                    }

                    if (FirstValue || TestValue > FieldDescriptor.MaxValue)
                    {
                        FieldDescriptor.MaxValue = TestValue;
                    }
                }

                if (!ObservedANullValue && TestValue == ANativeNullValue)
                {
                    ObservedANullValue = true;
                }

                FirstValue = false;
            }

            // If the data stream processed contained no null values, then force the
            // nullable flag to false so we don't encode an extra token for a null value
            // that will never be written.
            if (!ObservedANullValue)
            {
                FieldDescriptor.Nullable = false;
            }

            if (FieldDescriptor.Nullable && (FieldDescriptor.MaxValue != FieldDescriptor.NativeNullValue))
            {
                FieldDescriptor.MaxValue++;
                FieldDescriptor.EncodedNullValue = FieldDescriptor.MaxValue;
            }
            else
            {
                FieldDescriptor.EncodedNullValue = 0;
            }

            FieldDescriptor.CalculateRequiredBitFieldSize();
        }
    }
}
