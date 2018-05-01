using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BookingSystem.API.Helpers
{
    /// <summary>
    /// Provides means of copying or updating properties from objects to others
    /// </summary>
    public static class ObjectMapper
    {
        public enum UpdateFlag
        {
            //  Default
            None = 0,

            //  Will check whether the source property is null and prevent updating target property
            DeferUpdateOnNull = 1,

            //  Marks empty strings as null. This flag is usually combined with 'DeferUpdateOnNull'
            DenoteEmptyStringsAsNull = 2
        }

        /// <summary>
        /// Copies the values of properties from the source object to the target object through reflection and returns a collection of updated properties.
        /// The source object and the target object need not to be of the same type.
        /// </summary>
        /// <param name="source">The source object containing the properties you wish to copy</param>
        /// <param name="target">The target object(destination) where the values are copied to. The source</param>
        /// <param name="bindingFlags">Indicates how properties are discovered within source object type</param>
        /// <param name="updateFlags">Additional flags that determine how the target object properties are updated</param>
        /// <param name="only">When this paramerter is set, only matching properties are used</param>
        /// <param name="exclude">When set, properties matching any member is ignored</param>
        /// <returns>The updated properties</returns>
        public static string[] CopyPropertiesTo(object source, object target, UpdateFlag updateFlags = UpdateFlag.None, string[] only = null, string[] exclude = null, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            var targetType = target.GetType();
            var srcType = source.GetType();

            var properties = targetType.GetProperties(bindingFlags);
            if (only != null)
            {
                properties = properties.Where(x => only.Contains(x.Name)).ToArray();
            }

            if (exclude != null)
            {
                properties = properties.Where(x => !exclude.Contains(x.Name)).ToArray();
            }

            List<string> updatedProperties = new List<string>(properties.Count() + 1);

            if (srcType == targetType)
            {
                foreach (var property in properties)
                {
                    var val = property.GetValue(source);
                    if ((updateFlags.HasFlag(UpdateFlag.DeferUpdateOnNull) && val == null) ||
                        (updateFlags.HasFlag(UpdateFlag.DenoteEmptyStringsAsNull) && property.PropertyType == typeof(string) && string.IsNullOrEmpty(val?.ToString())))
                        continue;

                    if (val?.Equals(property.GetValue(target)) == false)
                    {
                        property.SetValue(target, property.GetValue(source));
                        updatedProperties.Add(property.Name);
                    }
                }
            }
            else
            {
                foreach (var property in properties)
                {
                    var equivalent = srcType.GetProperty(property.Name, bindingFlags);
                    if (equivalent != null)
                    {
                        var val = equivalent.GetValue(source);
                        if ((updateFlags.HasFlag(UpdateFlag.DeferUpdateOnNull) && val == null) ||
                         (updateFlags.HasFlag(UpdateFlag.DenoteEmptyStringsAsNull) && property.PropertyType == typeof(string) && string.IsNullOrEmpty(val?.ToString())))
                            continue;

                        if (val?.Equals(property.GetValue(target)) == false)
                        {
                            property.SetValue(target, equivalent.GetValue(source));
                            updatedProperties.Add(property.Name);
                        }
                    }
                }
            }

            return updatedProperties.ToArray();

        }
    }
}