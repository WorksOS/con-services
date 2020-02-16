using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace VSS.Hosted.VLCommon
{
  public class SqlReaderAccess
  {
    private static DateTime GetDateTime( SqlDataReader reader, int index )
    {
      DateTime eventTime = DateTime.MinValue;

      if ( !reader.IsDBNull( index ) )
      {
        eventTime = reader.GetDateTime( index );
      }
      return eventTime;
    }
    public static DateTime GetDateTime( SqlDataReader reader, string fieldName )
    {
      return GetDateTime(reader,reader.GetOrdinal(fieldName));
    }
    public static DateTime? GetNullableDateTime( SqlDataReader reader, int index )
    {
      DateTime? value = null;
      if ( !reader.IsDBNull( index ) )
      {
        value = reader.GetDateTime( index );
      }
      return value;
    }
    public static DateTime? GetNullableDateTime( SqlDataReader reader, string fieldName )
    {
      return GetNullableDateTime( reader, reader.GetOrdinal( fieldName ) );
    }

    public static SqlGeometry GetGeometryFromWKT(SqlDataReader reader, int index)
    {
      string wkt = GetString(reader, index);
      return SqlGeometry.STGeomFromText(new SqlChars(new SqlString(wkt)), wgs84SRID);
    }
    public static SqlGeometry GetGeometryFromWKT(SqlDataReader reader, string fieldName)
    {
      return GetGeometryFromWKT(reader, reader.GetOrdinal(fieldName));
    }

    public static Guid GetGuid(SqlDataReader reader, string fieldName)
    {
      return GetGuid(reader, reader.GetOrdinal(fieldName));
    }

    private static Guid GetGuid(SqlDataReader reader, int index)
    {
      Guid eventValue = Guid.Empty;

      if (!reader.IsDBNull(index))
      {
        eventValue = reader.GetGuid(index);
      }
      return eventValue;
    }

    public static Guid? GetNullableGuid(SqlDataReader reader, string fieldName)
    {
      return GetNullableGuid(reader, reader.GetOrdinal(fieldName));
    }

    private static Guid? GetNullableGuid(SqlDataReader reader, int index)
    {
      Guid? eventValue = null;

      if (!reader.IsDBNull(index))
      {
        eventValue = reader.GetGuid(index);
      }
      return eventValue;
    }

    private static long GetLong( SqlDataReader reader, int index )
    {
      long eventValue = -1;

      if( !reader.IsDBNull( index ) )
      {
        eventValue = long.Parse( reader.GetValue( index ).ToString() );
      }
      return eventValue;
    }
    public static long GetLong( SqlDataReader reader, string fieldName )
    {
      return GetLong( reader, reader.GetOrdinal( fieldName ) );
    }

    private static long? GetNullableLong( SqlDataReader reader, int index )
    {
      long? eventValue = null;

      if ( !reader.IsDBNull( index ) )
        eventValue = long.Parse( reader.GetValue( index ).ToString() );
      return eventValue;
    }
    public static long? GetNullableLong( SqlDataReader reader, string fieldName )
    {
      return GetNullableLong( reader, reader.GetOrdinal( fieldName ) );
    }

    private static int GetInt( SqlDataReader reader, int index )
    {
      int val = 0;

      if ( !reader.IsDBNull( index ) )
      {
        val = int.Parse( reader.GetValue( index ).ToString() );
      }
      return val;
    }
    public static int GetInt( SqlDataReader reader, string fieldName )
    {
      return GetInt( reader, reader.GetOrdinal( fieldName ) );
    }

    private static int? GetNullableInt( SqlDataReader reader, int index )
    {
      int? val = null;

      int intVal;
      if ( !reader.IsDBNull( index ) && int.TryParse(reader.GetValue(index).ToString(), out intVal) )
      {
        val = intVal;
      }
      return val;
    }
    public static int? GetNullableInt( SqlDataReader reader, string fieldName )
    {
      return GetNullableInt( reader, reader.GetOrdinal( fieldName ) );
    }

    private static bool GetBool( SqlDataReader reader, int index )
    {
      bool val = false;

      if ( !reader.IsDBNull( index ) )
      {
        val = bool.Parse( reader.GetValue( index ).ToString() );
      }
      return val;
    }
    public static bool GetBool( SqlDataReader reader, string fieldName )
    {
      return GetBool( reader, reader.GetOrdinal( fieldName ) );
    }

    private static double GetDouble( SqlDataReader reader, int index )
    {
      double eventValue = double.NaN;

      if( !reader.IsDBNull( index ) )
      {
        eventValue = double.Parse( reader.GetValue( index ).ToString() );
      }
      return eventValue;
    }
    public static double? GetNullableDouble( SqlDataReader reader, string fieldName )
    {
      return GetNullableDouble(reader,reader.GetOrdinal(fieldName));
    }
    private static double? GetNullableDouble(SqlDataReader reader, int index)
    {
      double? eventValue = null;

      if (!reader.IsDBNull(index))
      {
        eventValue = double.Parse(reader.GetValue(index).ToString());
      }
      return eventValue;
    }
    public static double GetDouble(SqlDataReader reader, string fieldName)
    {
      return GetDouble(reader, reader.GetOrdinal(fieldName));
    }

    private static string GetString( SqlDataReader reader, int index )
    {
      string eventText = null;

      if( !reader.IsDBNull( index ) )
      {
        eventText = reader.GetString( index );
      }
      return eventText;
    }
    public static string GetString( SqlDataReader reader, string fieldName )
    {
      return GetString(reader,reader.GetOrdinal(fieldName));
    }

    //private static SqlGeometry GetGeometry(SqlDataReader reader, int index)
    //{
    //  SqlGeometry eventValue = null;
    //  if (!reader.IsDBNull(index))
    //  {
    //    eventValue = reader[index] as SqlGeometry;
    //  }
    //  return eventValue;
    //}
    //public static SqlGeometry GetGeometry(SqlDataReader reader, string fieldName)
    //{
    //  return GetGeometry(reader, reader.GetOrdinal(fieldName));
    //}

    private static T GetEnum<T>( SqlDataReader reader, int index )
    {
      T type = default(T);

      if ( !reader.IsDBNull( index ) )
      {
        type = ( T )Enum.Parse( typeof( T ), ( reader.GetValue( index ).ToString() ), true );
      }
      return type;
    }
    public static T GetEnum<T>( SqlDataReader reader, string fieldName )
    {
      return GetEnum<T>( reader, reader.GetOrdinal( fieldName ) );
    }

    public static IEnumerable<T> Read<T>( StoredProcDefinition sp, ReadMethod<T> d )
    {
      using ( SqlDataReader reader = SqlAccessMethods.ExecuteReader( sp ) )
      {
        while ( reader.Read() )
        {
          yield return d( reader );
        }
      }
    }

    public static IDictionary<K, V> Read<K, V>(StoredProcDefinition sp, KeyMethod<K> k, ValueMethod<V> v)
    {
      IDictionary<K, V> dictionary = new Dictionary<K, V>();
      using (SqlDataReader reader = SqlAccessMethods.ExecuteReader(sp))
      {
        while (reader.Read())
        {
          dictionary.Add(k( reader), v( reader));
        }
      }
      return dictionary;
    }

    public static IList<T> ToList<T>( StoredProcDefinition sp, ReadMethod<T> d )
    {
      IList<T> list = new List<T>();
      using ( SqlDataReader reader = SqlAccessMethods.ExecuteReader( sp ) )
      {
        while ( reader.Read() )
        {
          list.Add( d( reader ) );
        }
      }

      return list;
    }

    public static T Singular<T>( StoredProcDefinition sp, ReadMethod<T> d )
    {
      IList<T> list = new List<T>();
      using ( SqlDataReader reader = SqlAccessMethods.ExecuteReader( sp ) )
      {
        while ( reader.Read() )
        {
          list.Add( d( reader ) );
        }
      }

      return list.Count == 1 ? list[0] : default(T);
    }

    public delegate T ReadMethod<T>(SqlDataReader reader);
    public delegate K KeyMethod<K>(SqlDataReader reader);
    public delegate V ValueMethod<V>(SqlDataReader reader);


    private static readonly int wgs84SRID = 4326;
  }
}
