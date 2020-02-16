using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
  /// <summary>
  /// Creates and hydrates an object with random non-null data.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObjectGenerator<T>
    where T : new()
  {
    private class PropertyAccessor
    {
      public Type DeclaringType { get; set; }
      public string Name { get; set; }
      public Type PropertyType { get; set; }
      public Type RealPropertyType
      {
        get
        {
          return this.PropertyType.GetTypeOrNullableType();
        }
      }
      public Func<object> Get { get; set; }
      public Action<object> Set { get; set; }
    }

    private List<PropertyAccessor> accessors = new List<PropertyAccessor>();
    private T obj;
    private Random random = new Random();

    public T Generate(int? seed = null)
    {
      obj = new T();
      if (seed != null)
      {
        random = new Random(seed.Value);
      }
      this.accessors = new List<PropertyAccessor>();
      this.FillObject(obj);
      return obj;
    }

    private void FillObject(object obj)
    {
      foreach (MemberInfo member in obj.GetType().GetMembers())
      {
        PropertyAccessor accessor = this.BuildAccessor(member, obj);
        if (accessor == null) continue;

        this.accessors.Add(accessor);
        this.SetRandom(accessor);

        object child = accessor.Get();
        this.FillObject(child);

        if (typeof(IEnumerable).IsAssignableFrom(child.GetType()))
        {
          // fill each element
          foreach (object item in (IEnumerable)child)
          {
            this.FillObject(item);
          }
        }
      }
    }

    private PropertyAccessor BuildAccessor(MemberInfo member, object obj)
    {
      if (!(member is PropertyInfo || member is FieldInfo)) return null;

      PropertyAccessor accessor = new PropertyAccessor();
      if (member is PropertyInfo)
      {
        PropertyInfo property = member as PropertyInfo;
        if (property.GetSetMethod() == null || property.GetSetMethod().IsPublic == false || property.GetIndexParameters().Length > 0)
        {
          return null;
        }
        accessor.PropertyType = property.PropertyType;
        accessor.DeclaringType = property.DeclaringType;
        accessor.Name = property.Name;
        accessor.Get = () => property.GetValue(obj);
        accessor.Set = (value) => property.SetValue(obj, value);
      }
      else if (member is FieldInfo)
      {
        FieldInfo field = member as FieldInfo;
        if (field.IsInitOnly || field.IsSpecialName || field.IsLiteral)
        {
          return null;
        }
        accessor.PropertyType = field.FieldType;
        accessor.DeclaringType = field.DeclaringType;
        accessor.Name = field.Name;
        accessor.Get = () => field.GetValue(obj);
        accessor.Set = (value) => field.SetValue(obj, value);
      }
      else
      {
        return null;
      }
      return accessor;
    }

    private void SetRandom(PropertyAccessor accessor)
    {
      if (accessor.RealPropertyType == typeof(string))
      {
        accessor.Set(random.Next().ToString("x"));
      }
      else if (accessor.RealPropertyType == typeof(int))
      {
        accessor.Set(random.Next(100));
      }
      else if (accessor.RealPropertyType == typeof(bool))
      {
        accessor.Set(random.Next(2) == 0);
      }
      else if (accessor.RealPropertyType == typeof(DateTime))
      {
        accessor.Set(DateTime.UtcNow.Date
          .AddDays(-random.Next(3))
          .AddHours(-random.Next(24))
          .AddMinutes(-random.Next(60))
          .AddSeconds(-random.Next(60))
          .AddMilliseconds(-random.Next(1000)));
      }
      else if (accessor.RealPropertyType == typeof(TimeSpan))
      {
        accessor.Set(
          TimeSpan.FromSeconds(random.Next(60)) +
          TimeSpan.FromMinutes(random.Next(60)) +
          TimeSpan.FromHours(random.Next(23)));
      }
      else if (accessor.RealPropertyType.IsEnum)
      {
        Array possibleEnumValues = Enum.GetValues(accessor.RealPropertyType);
        accessor.Set(possibleEnumValues.GetValue(random.Next(possibleEnumValues.Length)));
      }
      else if (accessor.RealPropertyType.HasDefaultConstructor())
      {
        // if it has a default constuctor, use it
        accessor.Set(accessor.PropertyType.InvokeDefaultConstructor());
      }

      // populate collections
      if (accessor.RealPropertyType.ImplementsGenericInterface(typeof(ICollection<>)))
      {
        // this member is a collection, see if its member type has a default constructor
        Type itemType = accessor.PropertyType.GenericTypeArguments[0];
        if (itemType.HasDefaultConstructor())
        {
          // it has a default constructor, add some elements
          object list = accessor.Get();
          MethodInfo addMethod = list.GetType().GetMethod("Add", new Type[] { accessor.PropertyType.GenericTypeArguments[0] });
          for (int i = 0; i < random.Next(10) + 1; i++)
          {
            object newItem = itemType.InvokeDefaultConstructor();
            addMethod.Invoke(list, new object[] { newItem });
          }
        }
      }
    }
  }
}
