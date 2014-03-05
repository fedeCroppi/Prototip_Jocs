using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FaceTrackingBasics
{
    public class Xml
    {
        private XmlDocument doc;

        public Xml()
        {
            doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(docNode);
        }

        public List<string[]> LlegirXml()
        {
            List<string[]> dades = new List<string[]>();
            XmlTextReader reader = new XmlTextReader(@"Jugadors\jugadors.xml");
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "player")
                    {
                        string[] player = new string[5];
                        if (reader.HasAttributes)
                        {
                            
                            for (int i = 0; i < reader.AttributeCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                player[i] = reader.Value;
                            }
                            reader.MoveToElement();
                        }
                        if (reader.Name == "foto")
                        {
                            if (reader.HasAttributes)
                            {
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    player[i] = reader.Value;
                                }
                                reader.MoveToElement();
                            }
                        }
                        if (reader.Name == "skeleton")
                        {
                            if (reader.HasAttributes)
                            {
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    player[i] = reader.Value;
                                }
                                reader.MoveToElement();
                            }
                        }
                        dades.Add(player);
                    }
                }
            }
            return dades;
        }

        public void desarXML(ref List<string[]> dades)
        {
            XmlNode rootNode = doc.CreateElement("players");

            foreach (string[] aux in dades)
            {
                //Node player
                XmlNode nodePlayer = doc.CreateElement("player");

                //Atributs player
                XmlAttribute attrNom = doc.CreateAttribute("nom");
                attrNom.Value = aux[0];
                nodePlayer.Attributes.Append(attrNom);

                XmlAttribute attrCog = doc.CreateAttribute("cognom");
                attrCog.Value = aux[1];
                nodePlayer.Attributes.Append(attrCog);

                XmlAttribute attrHis = doc.CreateAttribute("nHistoria");
                attrHis.Value = aux[2];
                nodePlayer.Attributes.Append(attrHis);

                // Node Foto amb atribut
                XmlNode nodeFoto = doc.CreateElement("foto");
                XmlAttribute attrFoto = doc.CreateAttribute("nom");
                attrFoto.Value = aux[3];
                nodeFoto.Attributes.Append(attrFoto);
                nodePlayer.AppendChild(nodeFoto);

                // Node Skeleton amb atribut
                XmlNode nodeSkeleton = doc.CreateElement("skeleton");
                XmlAttribute attrSke = doc.CreateAttribute("nom");
                attrSke.Value = aux[4];
                nodeSkeleton.Attributes.Append(attrSke);
                nodePlayer.AppendChild(nodeSkeleton);

                rootNode.AppendChild(nodePlayer);
            }
            this.doc.AppendChild(rootNode);
            doc.Save(@"Jugadors/jugadors.xml");
        }
    }
}
