using Boo.Lang;
using System.Linq;
using UnityEngine;

public class LogParts : MonoBehaviour
{
    public void DoLog()
    {
        //To log:
        //1) Get all mast components    OK
        //2) Get BoatPartOptions out of them    OK
        //3) Calculate dimensions from bounds
        //4) Calculate weight / meters and price / meters for masts and stays
        //5) Log and tabulate nicely

        int nameWidth = 30;
        int heightWidth = 10;
        int massWidth = 10;
        int priceWidth = 10;
        int installWidth = 10;

        string header =
            "Part".PadRight(nameWidth) +
            "Height".PadRight(heightWidth) +
            "Mass".PadRight(massWidth) +
            "Price".PadRight(priceWidth) +
            "Install Cost".PadRight(installWidth);

        //Mast[] masts = GetComponentsInChildren<Mast>();
        Mast[] masts = FindObjectsOfType<Mast>();
        BoatPartOption[] mastOptions = new BoatPartOption[masts.Length];
        float[] heights = new float[masts.Length];
        float[] massRatio = new float[masts.Length];
        float[] priceRatio = new float[masts.Length];
        float[] installRatio = new float[masts.Length];
        float staysMass = 0f;
        float staysPrice = 0f;
        float staysInstall = 0f;
        float mastsMass = 0f;
        float mastsPrice = 0f;
        float mastsInstall = 0f;
        int staysCount = 0;
        int mastsCount = 0;

        string separator = new string('-', nameWidth + heightWidth + massWidth + priceWidth + installWidth + 2);
        string title = "LogParts: Printing Masts information:\n==== ==== MAST PARTS (" + masts.Length +") ==== ====";
        string staysString = "\n" + header + "\n" + separator;
        string mastsString = "\n" + header + "\n" + separator;

        for (int i = 0; i < masts.Length; i++)
        {
            if (!masts[i].shipRigidbody.name.Contains("50") && !masts[i].shipRigidbody.name.Contains("20") && !masts[i].shipRigidbody.name.Contains("70") && !masts[i].shipRigidbody.name.Contains("80")) continue;
            BoatPartOption part = masts[i].GetComponent<BoatPartOption>();
            if (part != null)
            {
                mastOptions[i] = part;
                Mesh mesh = part.GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null)
                {
                    heights[i] = mesh.bounds.extents.z * 2f;
                    massRatio[i] = part.mass / heights[i];
                    priceRatio[i] = part.basePrice / heights[i];
                    installRatio[i] = part.installCost / heights[i];
                    if (part.optionName.Contains("stay"))
                    {   //stays
                        staysString += "\n" +
                            part.optionName.PadRight(nameWidth) +
                            heights[i].ToString("0.0").PadRight(heightWidth) +
                            massRatio[i].ToString("0.0").PadRight(massWidth) +
                            priceRatio[i].ToString("0.0").PadRight(priceWidth) +
                            installRatio[i].ToString("0.0").PadRight(installWidth);
                        staysMass += massRatio[i];
                        staysPrice += priceRatio[i];
                        staysInstall += installRatio[i];
                        staysCount++;
                    }
                    else
                    {   //masts
                        mastsString += "\n" +
                            part.optionName.PadRight(nameWidth) +
                            heights[i].ToString("0.0").PadRight(heightWidth) +
                            massRatio[i].ToString("0.0").PadRight(massWidth) +
                            priceRatio[i].ToString("0.0").PadRight(priceWidth) +
                            installRatio[i].ToString("0.0").PadRight(installWidth);
                        mastsMass += massRatio[i];
                        mastsPrice += priceRatio[i];
                        mastsInstall += installRatio[i];
                        mastsCount++;
                    }
                }
            }
        }
        string log = title;
        string mastAverage = "\n" +
            "==== ==== MAST AVERAGES ==== ====" +
            "\nMass/Height: " + (mastsMass / mastsCount).ToString("0.0") +
            "\nPrice/Height: " + (mastsPrice / mastsCount).ToString("0.0") +
            "\nInstall/Height: " + (mastsInstall / mastsCount).ToString("0.0") + "\n";
        string stayAverage = "\n" +
            "==== ==== STAYS AVERAGES ==== ====" +
            "\nMass/Height: " + (staysMass / staysCount).ToString("0.0") +
            "\nPrice/Height: " + (staysPrice / staysCount).ToString("0.0") +
            "\nInstall/Height: " + (staysInstall / staysCount).ToString("0.0") + "\n";

        log += mastAverage + separator + mastsString + stayAverage + separator + staysString;
    
        Debug.Log(log);
    }
}
