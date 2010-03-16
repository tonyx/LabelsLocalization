using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LabelManager
{
    class SingletonLabelManager
    {
        private static string CLASS_NAME_PREFIX = "class";
  
        private JavaProperties props;

        private static SingletonLabelManager instance;

        protected SingletonLabelManager()
        {
            StreamReader streamReader = null;
            try
            {
                String assemblyPath = LabelUtils.GetCurrentAssemblyExecutionPath();
                streamReader = new StreamReader(File.Open(assemblyPath + "\\international.properties", FileMode.Open));
                props = new JavaProperties();
                props.Load(streamReader.BaseStream);

            }
            catch (Exception e)
            {

     //           log.Debug("eccezione in load international props " + e.StackTrace);

            }

        }


        /// <summary>
        /// this is apparently useful to not break ui visual studio forms
        /// </summary>
        /// <returns></returns>
        public ICollection GetKeyCollection()
        {
            if (props == null)
            {
                props = new JavaProperties();
                props.Add("bypass", "bypass");
            }


            return props.Keys;
        }

        public JavaProperties getProperties()
        {
            return props;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static SingletonLabelManager getInstance()
        {
            if (instance == null)
            {
                instance = new SingletonLabelManager();

            }
            return instance;
        }


        public String getLabel(String label)
        {
            try
            {
                return props.GetProperty(label, label);
            }
            catch (NullReferenceException e)
            {
                return label;
            }

        }
    }
}
