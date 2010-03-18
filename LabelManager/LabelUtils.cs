using System;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;


namespace LabelManager
{
    public class LabelUtils
    {
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>       
        public static string GetCurrentAssemblyExecutionPath()
        {
            String currentLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            int indexLast = currentLocation.LastIndexOf("\\");
            String toReturn = currentLocation.Remove(indexLast);
            return toReturn;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ob"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValueSubst"></param>
        private static void modifyValueOfFieldAccordingToTable(Object ob, String fieldName, String fieldValueSubst)
        {
            try
            {
                ob.GetType().GetProperty(fieldName).SetValue(ob, fieldValueSubst, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                ob.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).SetValue(ob, fieldValueSubst);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ob"></param>
        /// <param name="fieldPath"></param>
        /// <param name="fieldValue"></param>
        public static void modifyRecursively(Object ob, String fieldPath, String fieldValue)
        {
            if (ob == null)
            {
                return;
            }
            if (!fieldPath.Contains("."))
            {
                modifyValueOfFieldAccordingToTable(ob, fieldPath, fieldValue);
            }
            else
            {
                String prefix = fieldPath.Substring(0, fieldPath.IndexOf('.'));
                Object o;
                o = ob.GetType().GetProperty(prefix, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (o != null)
                {
                    PropertyInfo myProp = (PropertyInfo)o;
                    Object subObject = myProp.GetValue(ob, null);
                    String toPass = fieldPath.Substring(fieldPath.IndexOf('.') + 1);
                    modifyRecursively(subObject, toPass, fieldValue);
                }
                else
                {
                    o = ob.GetType().GetField(prefix, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    FieldInfo myField = (FieldInfo)o;
                    Object subObject = myField.GetValue(ob);
                    String toPass = fieldPath.Substring(fieldPath.IndexOf('.') + 1);
                    modifyRecursively(subObject, toPass, fieldValue);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        public static void UpdateUi(Object form)
        {
            String locale = System.Configuration.ConfigurationManager.AppSettings["locale"];
            if (locale == null || "".Equals(locale))
            {
                locale = "it";
            }


            String thisClassName = form.GetType().ToString();
            ICollection keys = SingletonLabelManager.getInstance().GetKeyCollection();
            IEnumerator keysEnum = keys.GetEnumerator();

            while (keysEnum.MoveNext())
            {
                String currentkey = (String)keysEnum.Current;
                String subStringOfKeyToMatch = locale + "." + thisClassName;

                if (currentkey.StartsWith(subStringOfKeyToMatch))
                {
                    String currentValue = (SingletonLabelManager.getInstance().getLabel(currentkey));
                    String localClassPropertyOrFieldPath = currentkey.Replace(subStringOfKeyToMatch + ".", "");
                    try
                    {
                        modifyRecursively(form, localClassPropertyOrFieldPath, currentValue);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }
    }
}
