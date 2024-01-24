using System;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using isci.Allgemein;
using isci.Daten;
using isci.Beschreibung;

namespace isci.modulbasis
{
    public class Konfiguration : Parameter
    {
        public Konfiguration(string[] args) : base(args) {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var konfiguration = new Konfiguration(args);

            var ausfuehrungsmodell = new Ausführungsmodell(konfiguration);

            var structure = new Datenstruktur(konfiguration);

            var dm = new Datenmodell(konfiguration);
            var example = new dtInt32(0, "example");
            dm.Dateneinträge.Add(example);

            var beschreibung = new Modul(konfiguration.Identifikation, "isci.modulbasis", new ListeDateneintraege(){example});
            beschreibung.Name = "Modulbasis Ressource " + konfiguration.Identifikation;
            beschreibung.Beschreibung = "Modulbasis";
            beschreibung.Speichern();

            dm.Speichern(konfiguration);

            structure.DatenmodellEinhängen(dm);
            structure.DatenmodelleEinhängenAusOrdner(konfiguration.OrdnerDatenmodelle);
            structure.Start();
            
            while(true)
            {
                structure.Zustand.WertAusSpeicherLesen();

                if (ausfuehrungsmodell.ContainsKey(structure.Zustand.Value()))
                {
                    var schritt_param = (string)ausfuehrungsmodell[structure.Zustand.Value()].Parametrierung;

                    switch (schritt_param)
                    {
                        case "E":
                            {
                                
                                break;
                            }
                        case "A":
                            {
                                if ((System.Int32)example.value == 255)
                                {
                                    example.value = (System.Int32)0;
                                }
                                else
                                {
                                    example.value = (System.Int32)example.value + 1;
                                }
                                break;
                            }
                    }

                    structure.Zustand++;
                    structure.Zustand.WertInSpeicherSchreiben();
                }
            }
        }
    }
}