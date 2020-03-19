using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace HouseOfGrey.MoveableStorage.Scripts
{
    public class BoxLabelTextService
    {
        private const string _fileName = "boxlabels.xml";
        private string GetFilePath()
        {
            var path = GameUtils.GetSaveGameDir(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName));

            return Path.Combine(path, _fileName);
        }

        private bool FileExtists()
        {
            return File.Exists(GetFilePath());
        }

        public void WriteText(string tileEntityPos, string text)
        {
            var cleanTileEntityPos = tileEntityPos.Replace(",", "").Replace(" ", "");
            var filePath = GetFilePath();
            if (!FileExtists())
            {
                var xmlDoc = new XmlDocument();
                var rootNode = xmlDoc.CreateElement("greysprophecy");
                xmlDoc.AppendChild(rootNode);

                var boxLabelsNode = xmlDoc.CreateElement("boxlabels");
                rootNode.AppendChild(boxLabelsNode);

                boxLabelsNode.AppendChild(CreateLabelElement(xmlDoc, cleanTileEntityPos, text));

                xmlDoc.Save(filePath);
            }
            else
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                var boxLabelNode = xmlDoc.SelectSingleNode("/greysprophecy/boxlabels/boxlabel[@entity_id=" + cleanTileEntityPos + "]");
                
                if(boxLabelNode != null)
                {
                    boxLabelNode.InnerText = text;
                }
                else
                {
                    var boxLabelsNode = xmlDoc.SelectSingleNode("/greysprophecy/boxlabels");
                    boxLabelsNode.AppendChild(CreateLabelElement(xmlDoc, cleanTileEntityPos, text));
                }

                xmlDoc.Save(filePath);
            }
        }

        public string ReadText(string tileEntityPos)
        {
            if (FileExtists())
            {
                var cleanTileEntityPos = tileEntityPos.Replace(",", "").Replace(" ", "");

                var filePath = GetFilePath();
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                var boxLabelNode = xmlDoc.SelectSingleNode("/greysprophecy/boxlabels/boxlabel[@entity_id=" + cleanTileEntityPos + "]");

                return boxLabelNode != null ? boxLabelNode.InnerText : "Storage";
            }

            return "Storage";
        }

        public void RemoveNode(string tileEntityPos)
        {
            if (FileExtists())
            {
                var cleanTileEntityPos = tileEntityPos.Replace(",", "").Replace(" ", "");

                var filePath = GetFilePath();
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                var boxLabelNode = xmlDoc.SelectSingleNode("/greysprophecy/boxlabels/boxlabel[@entity_id=" + cleanTileEntityPos + "]");

                if(boxLabelNode != null)
                {
                    var boxLabelsNode = xmlDoc.SelectSingleNode("/greysprophecy/boxlabels");
                    boxLabelsNode.RemoveChild(boxLabelNode);

                    xmlDoc.Save(filePath);
                }
            }
        }

        private XmlElement CreateLabelElement(XmlDocument xmlDoc, string tileEntityPos, string text)
        {
            var boxLabelNode = xmlDoc.CreateElement("boxlabel");
            var positionAttr = xmlDoc.CreateAttribute("position");
            var idAttr = xmlDoc.CreateAttribute("entity_id");
            idAttr.Value = tileEntityPos;
            boxLabelNode.Attributes.Append(idAttr);
            boxLabelNode.InnerText = text;

            return boxLabelNode;
        }
    }
}
