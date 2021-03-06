﻿using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

namespace LabelManager
{
    class SingletonLabelManager
    {
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
                // do what you want here
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public ICollection GetKeyCollection()
        {
            if (props == null)
            {
                props = new JavaProperties();
                props.Add("it.LabelManager.Sample.Attribute", "valore italiano");
                props.Add("en.LabelManager.Sample.Attribute", "valore inglese");
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
