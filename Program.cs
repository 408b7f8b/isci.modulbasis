using System;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using isci.Allgemein;
using isci.Daten;
using isci.Beschreibung;
using Newtonsoft.Json.Linq;

namespace isci.modulbasis
{
    /* Mit einer eigenen Konfigurationsklasse, die von isci.Allgemein.Parameter erbt, kann eine Parametrierung für das Modul erzeugt werden, die zusätzliche spezifische Felder besitzt.
    Die eigenen Felder werden aus Umgebungsvariablen, Ausführungsargumenten und Konfigurationsdatei gelesen, analog zu den Standardfeldern in isci.Allgemein.Parameter.
    Wichtig: es muss ein Konstruktor genutzt werden, der die Ausführungsargumente nutzt und an den Basiskonstruktor durchreicht (bspw. "public Konfiguration(string args[]): base(args)"). */
    public class Konfiguration : Parameter
    {
        public string Beispiel = "123";
        public Konfiguration(string[] args) : base(args) {

        }
    }

    public class testKlasse : dtObjekt
    {
        public dtString meinString;
        public testKlasse(string Identifikation) : base(Identifikation)
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Immer notwendig und in der Reihenfolge der isci-spezifischen Elemente immer zuerst: Erstellen der Parametrierung.
            var konfiguration = new Konfiguration(args);

            Logger.Alles("Eine nicht unbedingt notwendige Information, die zusätzlich bei der Modulentwicklung unterstützen kann.");
            Logger.Debug("Eine tiefe technische Information, die bei der Modulentwicklung unterstützt.");
            Logger.Information("Eine allgemeine Informationsangabe bei der Ausführung, beispielsweise für die Kontrolle der Parametrierung.");
            Logger.Warnung("Eine Warnung ist aufgetreten. Ein Hinweis, wenn beispielsweise technische Parameter unbelegt oder nicht in Ordnung sind.");
            Logger.Fehler("Ein Fehler ist aufgetreten. Zu verwenden, wenn schwerwiegende technische interne Fehler bei der Ausführung auftreten.");
            Logger.Fatal("Ein fataler Fehler ist aufgetreten. Zu verwenden, wenn die Modulinstanz nicht mehr ausgeführt werden kann.");

            //Erstellung des Zugriffs auf die dateibasierte Datenstruktur unter Nutzung der Parametrierung.
            var structure = new Datenstruktur(konfiguration);

            //Beispiel für die Erstellung eines Datenmodells. Wenn die Parametrierung genutzt wird, wird damit das Kerndatenmodell einer Modulinstanz erstellt mit der Modulinstanzidentifikation.
            //Die Modulinstanz kann auch weitere Datenmodelle erstellen unter Nutzung anderer Konstruktoren, sodass eine andere Identifikation genutzt wird.
            var dm = new Datenmodell(konfiguration);
            //Anlegen eines Dateneintrags und Hinzufügen zum Datenmodell.
            var example = new dtInt32(25, "example");
            dm.Add(example);
            var ausfdauer = new dtAusfuehrungsdauer();
            dm.Add(ausfdauer);
            var testObjekt = new testKlasse("testObjekt");
            dm.Add(testObjekt);
            
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
            var beschreibung = new Modul(konfiguration.Identifikation, "isci.modulbasis", dm.Dateneinträge)
            {
                Name = "Modulbasis zur Entwicklung ausgeführt auf " + konfiguration.Identifikation,
                Beschreibung = "Beispieltext für die Instanzbeschreibung und Konfigurationsinformationen wie " + konfiguration.Beispiel
            };
            beschreibung.Speichern();
            
            //Arbeitsschleife
            while(true)
            {
                structure.Zustand.WertAusSpeicherLesen(); //Aktualisieren des Zustandswertes der Datenstruktur.

                if (ausfuehrungsmodell.AktuellerZustandModulAktivieren()) //Abprüfen, ob das Ausführungsmodell für den Zustandswert die Modulinstanz vorsieht.
                {
                    ausfdauer.MessungStart();
                    var schritt_param = (string)ausfuehrungsmodell.ParameterAktuellerZustand(); //Abruf der Parameter für die Ausführung. Rückgabetyp ist Object und kann in eigene Typen gewandelt werden.

                    switch (schritt_param)
                    {
                        case "E":
                            {
                                
                                break;
                            }
                        case "A":
                            {
                                structure.Lesen();

                                if (example.Wert == 255)
                                {
                                    example.Wert = 0;
                                }
                                else
                                {
                                    example.Wert++;
                                }

                                testObjekt.meinString.Wert = example.WertSerialisieren();
                                example.WertInSpeicherSchreiben(); //alternativ auch structure.Schreiben();
                                testObjekt.meinString.WertInSpeicherSchreiben();
                                break;
                            }
                    }

                    ausfuehrungsmodell.Folgezustand();
                    structure.Zustand.WertInSpeicherSchreiben(); //Zustandswert in Datenstruktur übernehmen.
                    ausfdauer.MessungEnde();
                }

                //isci.Helfer.SleepForMicroseconds(konfiguration.PauseArbeitsschleifeUs);
                System.Threading.Thread.Sleep(1);
            }
        }
    }
}