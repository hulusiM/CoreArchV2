
function totalKm_trip(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart1", am4charts.XYChart);
    chart.data = data

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.ticks.template.disabled = true;
    categoryAxis.renderer.line.opacity = 0;
    categoryAxis.renderer.grid.template.disabled = true;
    categoryAxis.renderer.minGridDistance = 40;
    categoryAxis.dataFields.category = "FuelDate2";
    categoryAxis.startLocation = 0.4;
    categoryAxis.endLocation = 0.6;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";


    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.tooltip.disabled = true;
    valueAxis.renderer.line.opacity = 0;
    valueAxis.renderer.ticks.template.disabled = true;
    valueAxis.min = 0;

    var lineSeries = chart.series.push(new am4charts.LineSeries());
    lineSeries.dataFields.categoryX = "FuelDate2";
    lineSeries.dataFields.valueY = "Count";
    lineSeries.tooltipText = "Toplam: [bold]{valueY}[/] Km"
    lineSeries.fillOpacity = 0.5;
    lineSeries.strokeWidth = 3;
    lineSeries.propertyFields.stroke = "lineColor";
    lineSeries.propertyFields.fill = "lineColor";
    lineSeries.stroke = am4core.color("#F06292"); //çizgi rengi

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.behavior = "panX";
    chart.cursor.lineX.opacity = 0;
    chart.cursor.lineY.opacity = 0;

    //// Make bullets grow on hover
    var bullet = lineSeries.bullets.push(new am4charts.CircleBullet());
    bullet.circle.strokeWidth = 2;
    bullet.circle.radius = 6;
    bullet.circle.fill = am4core.color("#c4fe15");

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Ay Bazlı Toplam Km";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    //chart.numberFormatter.numberFormat = "#,###.##";

    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}


function totalTripCount_trip(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart2", am4charts.XYChart);
    chart.data = data

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.ticks.template.disabled = true;
    categoryAxis.renderer.line.opacity = 0;
    categoryAxis.renderer.grid.template.disabled = true;
    categoryAxis.renderer.minGridDistance = 40;
    categoryAxis.dataFields.category = "FuelDate2";
    categoryAxis.startLocation = 0.4;
    categoryAxis.endLocation = 0.6;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";


    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.tooltip.disabled = true;
    valueAxis.renderer.line.opacity = 0;
    valueAxis.renderer.ticks.template.disabled = true;
    valueAxis.min = 0;

    var lineSeries = chart.series.push(new am4charts.LineSeries());
    lineSeries.dataFields.categoryX = "FuelDate2";
    lineSeries.dataFields.valueY = "Count";
    lineSeries.tooltipText = "Toplam: [bold]{valueY}[/] Adet"
    lineSeries.fillOpacity = 0.5;
    lineSeries.strokeWidth = 3;
    lineSeries.propertyFields.stroke = "lineColor";
    lineSeries.propertyFields.fill = "lineColor";
    lineSeries.stroke = am4core.color("#F06292"); //çizgi rengi

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.behavior = "panX";
    chart.cursor.lineX.opacity = 0;
    chart.cursor.lineY.opacity = 0;

    //// Make bullets grow on hover
    var bullet = lineSeries.bullets.push(new am4charts.CircleBullet());
    bullet.circle.strokeWidth = 2;
    bullet.circle.radius = 6;
    bullet.circle.fill = am4core.color("#c4fe44");

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Ay Bazlı Görev Sayısı";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    //chart.numberFormatter.numberFormat = "#,###.##";

    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}

function totalTripCount_trip2(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart2", am4charts.XYChart3D);
    chart.data = data;

    // Create axes
    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "FuelDate2";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.minGridDistance = 30;
    categoryAxis.renderer.labels.template.horizontalCenter = "right";
    categoryAxis.renderer.labels.template.verticalCenter = "middle";
    categoryAxis.tooltip.disabled = false;
    categoryAxis.renderer.minHeight = 110;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Ay Bazlı Toplam Görev Sayısı";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "FuelDate2";
    series.dataFields.categoryX = "Count";
    series.tooltipText = "Görev Sayısı: [bold]{valueY}[/] Adet";
    series.columns.template.fillOpacity = .8;
    series.columns.template.fill = am4core.color("#F06292");

    var columnTemplate = series.columns.template;
    columnTemplate.strokeWidth = 2;
    columnTemplate.strokeOpacity = 1;
    columnTemplate.stroke = am4core.color("#FFFFFF");

    columnTemplate.adapter.add("fill",
        function (fill, target) {
            return chart.colors.getIndex(target.dataItem.index);
        });

    columnTemplate.adapter.add("stroke",
        function (stroke, target) {
            return chart.colors.getIndex(target.dataItem.index);
        });

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.lineX.strokeOpacity = 0;
    chart.cursor.lineY.strokeOpacity = 0;

    //if (data.length > 40 && data.length < 100)
    //    zoomAxis(categoryAxis, 0, '0.8');
    //else if (data.length > 100)
    //    zoomAxis(categoryAxis, 0, '0.2');

    //chart.numberFormatter.numberFormat = "#,###.##";

    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    //chart.legend = new am4charts.Legend();//Alt bilgi

    chart.exporting.menu = new am4core.ExportMenu();//export bilgileri
    DownloadIconCustom(chart);
}

function zoomAxis(categoryAxis, start, end) {
    categoryAxis.start = start;
    categoryAxis.end = end;
    categoryAxis.keepSelection = true;
}
