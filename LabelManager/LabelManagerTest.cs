using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LabelManager
{
    [TestFixture]
    class LabelManagerTest
    {

        [Test]
        public void TheNaturalMockOfSingletonLabelManagerContainsTwoElements()
        {
            Assert.AreEqual(2,SingletonLabelManager.getInstance().GetKeyCollection().Count);
        }

        [Test]
        public void TheAttributeIsChangedByReflection()
        {
            Sample sample = new Sample("");
            sample.Attribute = "string";
            Assert.AreEqual("string",sample.Attribute);
            LabelUtils.UpdateUi(sample);
            Assert.AreEqual("valore italiano", sample.Attribute);
        }
    }
    class Sample
    {
        private string privateField;
        public string PublicField;
        private string _attribute;

        public Sample(string privateField)
        {
            this.privateField = privateField;
        }

        public string Attribute
        {
            get { return _attribute; }
            set { _attribute = value; }
        }
    }
}
