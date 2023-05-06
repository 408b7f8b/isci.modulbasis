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
        public Konfiguration(string datei) : base(datei) {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var konfiguration = new Konfiguration("konfiguration.json");
            
            var structure = new Datenstruktur(konfiguration.OrdnerDatenstruktur);

            var dm = new Datenmodell(konfiguration.Identifikation);
            var example = new dtInt32(0, "example");
            dm.Dateneinträge.Add(example);

            var beschreibung = new Modul(konfiguration.Identifikation, "isci.modulbasis", new ListeDateneintraege(){example});
            beschreibung.Name = "Modulbasis Ressource " + konfiguration.Identifikation;
            beschreibung.Beschreibung = "Modulbasis";
            beschreibung.Speichern(konfiguration.OrdnerBeschreibungen + "/" + konfiguration.Identifikation + ".json");

            dm.Speichern(konfiguration.OrdnerDatenmodelle + "/" + konfiguration.Identifikation + ".json");

            structure.DatenmodellEinhängen(dm);
            structure.DatenmodelleEinhängenAusOrdner(konfiguration.OrdnerDatenmodelle);
            structure.Start();

            var Zustand = new dtZustand(konfiguration.OrdnerDatenstruktur);
            Zustand.Start();
            
            while(true)
            {
                Zustand.Lesen();

                var erfüllteTransitionen = konfiguration.Ausführungstransitionen.Where(a => a.Eingangszustand == (System.Int32)Zustand.value);
                if (erfüllteTransitionen.Count<Ausführungstransition>() <= 0) continue;

                if ((System.Int32)Zustand.value == 0)
                {
                    example.value = 0;
                } else {
                    example.value = 1;
                }
                structure.Schreiben();

                Zustand.value = erfüllteTransitionen.First<Ausführungstransition>().Ausgangszustand;
                Zustand.Schreiben();
            }
        }
    }
}