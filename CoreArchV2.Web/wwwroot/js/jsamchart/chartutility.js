function DownloadIconCustom(chart) {
    chart.exporting.menu.items = [
        {
            label:
                '<i style="padding:7px;" class="gd-download" aria-hidden="true"></i>',
            menu: [
                {
                    label: "Yazdır",
                    type: "print"
                },
                //{
                //    label: "Veri",
                //    menu: [
                //        { type: "json", label: "JSON" },
                //        { type: "csv", label: "CSV" },
                //        { type: "xlsx", label: "XLSX" },
                //        { type: "pdfdata", label: "PDF" }
                //    ]
                //},
                {
                    label: "Resim",
                    menu: [
                        { type: "png", label: "PNG" },
                        { type: "jpg", label: "JPG" },
                        { type: "gif", label: "GIF" },
                        { type: "svg", label: "SVG" },
                        //{ type: "pdf", label: "PDF" }
                    ]
                },
                //{
                //    label: "Tam Ekran",
                //    type: "custom",
                //    options: {
                //        callback: function () {
                //            $("#panel-fullscreen").click();
                //        }
                //    }
                //}
            ]
        }
    ];
}