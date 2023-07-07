using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;
using UNIHper;

namespace UNIPlayer
{
    public enum UNIAlign
    {
        lefttop,
        top,
        righttop,
        right,
        rightbottom,
        bottom,
        leftbottom,
        left,
        center
    }

    public class UNITransform
    {
        [DefaultValueAttribute(UNIAlign.center)]
        [XmlAttribute]
        public UNIAlign pivot = UNIAlign.center;

        [DefaultValueAttribute(UNIAlign.center)]
        [XmlAttribute]
        public UNIAlign align = UNIAlign.center;

        [DefaultValueAttribute(0)]
        [XmlAttribute]
        public float x = 0;

        [DefaultValueAttribute(0)]
        [XmlAttribute]
        public float y = 0;

        [DefaultValueAttribute(100)]
        [XmlAttribute]
        public float width = 100;

        [DefaultValueAttribute(100)]
        [XmlAttribute]
        public float height = 100;

        [DefaultValueAttribute(1.0f)]
        [XmlAttribute]
        public float scale = 1.0f;

        [DefaultValueAttribute(true)]
        [XmlAttribute]
        public bool visible = true;
    }

    public class UNIImage : UNITransform
    {
        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string url = "";

        [DefaultValueAttribute(false)]
        [XmlAttribute]
        public bool match = false;
    }

    public class UNIButton : UNIImage
    {
        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onClick = "";
    }
}
