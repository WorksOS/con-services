using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;

namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// Tracks cut anf fille volume information...
    /// </summary>
    public class CutFillVolume
    {
        private double? _CutVolume = null;
        public double CutVolume
        {
            get
            {
                return _CutVolume ?? 0;
            }
        }

        private double? _FillVolume = null;
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
            _CutVolume = CutVolume;
            _FillVolume = FillVolume;
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

        public void AddCutFillVolume(double CutVolume, double FillVolume)
        {
            AddCutVolume(CutVolume);
            AddFillVolume(FillVolume);
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

        public void AddVolume(CutFillVolume Volume)
        {
            if (Volume.HasCutVolume)
                AddCutVolume(Volume.CutVolume);

            if (Volume.HasFillVolume)
                AddFillVolume(Volume.FillVolume);
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
