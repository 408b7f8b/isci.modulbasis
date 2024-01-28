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
    /* Mit einer eigenen Konfigurationsklasse, die von isci.Allgemein.Parameter erbt, kann eine Parametrierung für das Modul erzeugt werden, die zusätzliche spezifische Felder besitzt.
    Die eigenen Felder werden aus Umgebungsvariablen, Ausführungsargumenten und Konfigurationsdatei gelesen, analog zu den Standardfeldern in isci.Allgemein.Parameter.
    Wichtig: es muss ein Konstruktor genutzt werden, der die Ausführungsargumente nutzt und an den Basiskonstruktor durchreicht (bspw. "public Konfiguration(string args[]): base(args)"). */
    public class Konfiguration : Parameter
    {
        public string Beispiel;
        public Konfiguration(string[] args) : base(args) {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Immer notwendig und in der Reihenfolge der isci-spezifischen Elemente immer zuerst: Erstellen der Parametrierung.
            var konfiguration = new Konfiguration(args);

            //Erstellung des Zugriffs auf die dateibasierte Datenstruktur unter Nutzung der Parametrierung.
            var structure = new Datenstruktur(konfiguration);

            //Beispiel für die Erstellung eines Datenmodells. Wenn die Parametrierung genutzt wird, wird damit das Kerndatenmodell einer Modulinstanz erstellt mit der Modulinstanzidentifikation.
            //Die Modulinstanz kann auch weitere Datenmodelle erstellen unter Nutzung anderer Konstruktoren, sodass eine andere Identifikation genutzt wird.
            var dm = new Datenmodell(konfiguration);
            //Anlegen eines Dateneintrags und Hinzufügen zum Datenmodell.
            var example = new dtInt32(0, "example");
            dm.Dateneinträge.Add(example);
            //Speichern des Datenmodells im Standardordner als Datei.
            dm.Speichern(konfiguration);

            //Hinzufügen des Datenmodells zur Datenstruktur.
            structure.DatenmodellEinhängen(dm);
            //Hinzufügen aller als Dateien gespeicherte Datenmodelle im Standardordner.
            structure.DatenmodelleEinhängenAusOrdner(konfiguration.OrdnerDatenmodelle);
            //Logischer Start der Datenstruktur.
            structure.Start();

            //Einlesen des Ausführungsmodells bzw. des notwendigen Parts für das Ausführungsmodell und Verbindung mit dem Zustandsdateneintrag der Datenstruktur.
            var ausfuehrungsmodell = new Ausführungsmodell(konfiguration, structure.Zustand);            

            //Erstellung einer Beschreibung für die Modulinstanz und Ablegen als Datei im Standardordner.
            var beschreibung = new Modul(konfiguration.Identifikation, "isci.modulbasis", new ListeDateneintraege(){example});
            beschreibung.Name = "Modulbasis Ressource " + konfiguration.Identifikation;
            beschreibung.Beschreibung = konfiguration.Beispiel;
            beschreibung.Speichern();
            
            //Arbeitsschleife
            while(true)
            {
                structure.Zustand.WertAusSpeicherLesen(); //Aktualisieren des Zustandswertes der Datenstruktur.

                if (ausfuehrungsmodell.AktuellerZustandModulAktivieren()) //Abprüfen, ob das Ausführungsmodell für den Zustandswert die Modulinstanz vorsieht.
                {
                    var schritt_param = (string)ausfuehrungsmodell.ParameterAktuellerZustand(); //Abruf der Parameter für die Ausführung. Ist ein Object und kann in eigene Typen gewandelt werden.

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

                    structure.Zustand++; //Zustandswert inkrementieren.
                    structure.Zustand.WertInSpeicherSchreiben(); //Zustandswert in Datenstruktur übernehmen.
                }
            }
        }
    }
}