﻿using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class CalculateDesignProfileResponse : BaseRequestResponse, IEquatable<CalculateDesignProfileResponse>
  {
    private const byte versionNumber = 1;

    public List<XYZS> Profile { get; set; } = new List<XYZS>();

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(versionNumber);

      var profileLength = Profile?.Count ?? 0;
      writer.WriteInt(profileLength);
      if (profileLength > 0)
      {
        foreach (var pt in Profile)
        {
          writer.WriteDouble(pt.X);
          writer.WriteDouble(pt.Y);
          writer.WriteDouble(pt.Z);
          writer.WriteDouble(pt.Station);
          writer.WriteInt(pt.TriIndex);
        }
      }
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte version = reader.ReadByte();

      if (version != versionNumber)
        throw new ArgumentException($"Version {version} not valid for deserializing {nameof(CalculateDesignProfileResponse)}");

      var count = reader.ReadInt();
      Profile = new List<XYZS>(count);
      for (int i = 0; i < count; i++)
      {
        Profile.Add(new XYZS
        {
          X = reader.ReadDouble(),
          Y = reader.ReadDouble(),
          Z = reader.ReadDouble(),
          Station = reader.ReadDouble(),
          TriIndex = reader.ReadInt()
        });
      }
    }

    public bool Equals(CalculateDesignProfileResponse other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      if (Profile.Count != other.Profile.Count) return false;

      return !Profile.Where((pt, i) => !pt.Equals(other.Profile[i])).Any();
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CalculateDesignProfileResponse) obj);
    }

    public override int GetHashCode()
    {
      return (Profile != null ? Profile.GetHashCode() : 0);
    }
  }
}
