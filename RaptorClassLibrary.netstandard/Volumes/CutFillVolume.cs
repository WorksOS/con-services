using System.Diagnostics;

namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// Tracks cut anf fille volume information...
    /// </summary>
    public class CutFillVolume
    {
        private double? _CutVolume;
        public double CutVolume
        {
            get
            {
                return _CutVolume ?? 0;
            }
        }

        private double? _FillVolume;
        public double FillVolume
        {
            get
            {
                return _FillVolume ?? 0;
            }
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public CutFillVolume()
        {
        }

        public CutFillVolume(double cutVolume, double fillVolume) : this()
        {
            _CutVolume = cutVolume;
            _FillVolume = fillVolume;
        }

        public double AccumulatedVolume()
        {
            double Result = 0;

            if (HasCutVolume)
            {
                Result += CutVolume;
            }

            if (HasFillVolume)
            {
                Result += FillVolume;
            }

            return Result;
        }

        public double AccumulatedVolume_BulkShrinkAdjusted(double BulkageFactor, double ShrinkageFactor)
            => CutVolume_BulkageAdjusted(BulkageFactor) + FillVolume_ShrinkageAdjusted(ShrinkageFactor);

        public void AddCutFillVolume(double cutVolume, double fillVolume)
        {
            AddCutVolume(cutVolume);
            AddFillVolume(fillVolume);
        }

        public void AddCutVolume(double Volume)
        {
            Debug.Assert(Volume >= 0);

            _CutVolume = _CutVolume.HasValue ? _CutVolume + Volume : Volume;
        }

        public void AddFillVolume(double Volume)
        {
            Debug.Assert(Volume >= 0);

            _FillVolume = _FillVolume.HasValue ? _FillVolume + Volume : Volume;
        }

        public void AddVolume(CutFillVolume volume)
        {
            if (volume.HasCutVolume)
                AddCutVolume(volume.CutVolume);

            if (volume.HasFillVolume)
                AddFillVolume(volume.FillVolume);
        }

        public void Assign(CutFillVolume source)
        {
            _CutVolume = source.CutVolume;
            _FillVolume = source.FillVolume;
        }

        public double ExcessVolume() => CutVolume - FillVolume;

        public double ExcessVolume_BulkShrinkAdjusted(double BulkageFactor, double ShrinkageFactor) 
            => CutVolume_BulkageAdjusted(BulkageFactor) - FillVolume_ShrinkageAdjusted(ShrinkageFactor);

        public double CutVolume_BulkageAdjusted(double BulkageFactor) => CutVolume * BulkageFactor;

        public double FillVolume_ShrinkageAdjusted(double ShrinkageFactor) => FillVolume * ShrinkageFactor;

        public bool HasCutVolume => _CutVolume.HasValue;

        public bool HasFillVolume => _FillVolume.HasValue;

        public bool HasAccumulatedVolume => _CutVolume.HasValue || _FillVolume.HasValue;
    }
}
