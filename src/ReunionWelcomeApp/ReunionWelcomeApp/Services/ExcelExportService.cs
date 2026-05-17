using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ReunionWelcomeApp.Models;

namespace ReunionWelcomeApp.Services;

public static class ExcelExportService
{
    private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    private static readonly string ExcelPath = Path.Combine(DataDir, "asistentes.xlsx");
    private static readonly string CsvPath = Path.Combine(DataDir, "asistentes.csv");

    static ExcelExportService()
    {
        if (!Directory.Exists(DataDir))
            Directory.CreateDirectory(DataDir);
    }

    public static void SaveAttendees(List<Attendee> attendees)
    {
        try
        {
            var sorted = attendees.OrderBy(a => a.FullName).ToList();
            SaveExcel(sorted);
            SaveCsv(sorted);
            LoggingService.Log($"Saved {attendees.Count} attendees");
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to save attendees", ex);
        }
    }

    private static void SaveExcel(List<Attendee> attendees)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Asistentes");
        ws.Cell(1, 1).Value = "Nombre";
        ws.Cell(1, 2).Value = "Hora de llegada";

        for (int i = 0; i < attendees.Count; i++)
        {
            ws.Cell(i + 2, 1).Value = attendees[i].FullName;
            ws.Cell(i + 2, 2).Value = attendees[i].ArrivalTime.ToString("HH:mm:ss");
        }
        ws.Columns().AdjustToContents();
        workbook.SaveAs(ExcelPath);
    }

    private static void SaveCsv(List<Attendee> attendees)
    {
        using var writer = new StreamWriter(CsvPath);
        writer.WriteLine("Nombre,Hora de llegada");
        foreach (var a in attendees)
            writer.WriteLine($"\"{a.FullName}\",{a.ArrivalTime:HH:mm:ss}");
    }

    public static List<Attendee> LoadAttendees()
    {
        var attendees = new List<Attendee>();
        try
        {
            if (File.Exists(CsvPath))
            {
                var lines = File.ReadAllLines(CsvPath).Skip(1);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                        attendees.Add(new Attendee { FullName = parts[0].Trim('"') });
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to load attendees", ex);
        }
        return attendees;
    }
}