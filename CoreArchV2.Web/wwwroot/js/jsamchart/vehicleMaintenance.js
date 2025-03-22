// Ay bazlı bakım
function chart1_vehicleMain(data) {
    am4core.useTheme(am4themes_dark);
    am4core.useTheme(am4themes_animated);

    var chart = am4core.create("chart1", am4charts.XYChart);
    chart.data = data

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.ticks.template.disabled = true;
    categoryAxis.renderer.line.opacity = 0;
    categoryAxis.renderer.grid.template.disabled = true;
    categoryAxis.renderer.minGridDistance = 40;
    categoryAxis.dataFields.category = "InvoiceDate2";
    categoryAxis.startLocation = 0.4;
    categoryAxis.endLocation = 0.6;

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.tooltip.disabled = true;
    valueAxis.renderer.line.opacity = 0;
    valueAxis.renderer.ticks.template.disabled = true;
    valueAxis.min = 0;

    var lineSeries = chart.series.push(new am4charts.LineSeries());
    lineSeries.dataFields.categoryX = "InvoiceDate2";
    lineSeries.dataFields.valueY = "Amount";
    lineSeries.tooltipText = "Toplam: [bold]{valueY}[/] ₺";
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

    //var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Ay Bazlı Bakım/Onarım";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    chart.numberFormatter.numberFormat = "#,###.##";

    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}

// Araç bazlı toplam tutar bakım
function chart2_vehicleMain(data) {
    am4core.useTheme(am4themes_dark);
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart2", am4charts.XYChart3D);
    chart.data = data;

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "Plate";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.minGridDistance = 30;
    categoryAxis.renderer.labels.template.horizontalCenter = "right";
    categoryAxis.renderer.labels.template.verticalCenter = "middle";
    categoryAxis.tooltip.disabled = false;
    categoryAxis.renderer.minHeight = 110;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Araç Bazlı Bakım/Onarım";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    // Create series
    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Amount";
    series.dataFields.categoryX = "Plate";
    series.tooltipText = "Toplam Tutar: [bold]{valueY}[/] ₺";
    series.columns.template.fillOpacity = .8;
    series.columns.template.fill = am4core.color("#039BE5");

    createSeries("UserFaultAmount", "Kullanıcı Hatası", chart, "#FFFFFF");

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.lineX.strokeOpacity = 0;
    chart.cursor.lineY.strokeOpacity = 0;

    chart.numberFormatter.numberFormat = "#,###.##";
    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.scrollbarY = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    //chart.legend = new am4charts.Legend();//Alt bilgi
    DownloadIconCustom(chart);
}

// Müdürlük bazında bakım
function chart3_vehicleMain(data) {
    am4core.useTheme(am4themes_dark);
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart3", am4charts.PieChart3D);
    chart.data = data
    chart.radius = am4core.percent(60);
    chart.innerRadius = am4core.percent(35);
    chart.depth = 80;

    var series = chart.series.push(new am4charts.PieSeries3D());
    series.dataFields.value = "Amount";
    series.dataFields.depthValue = "Amount";
    series.dataFields.category = "UnitName";
    series.slices.template.cornerRadius = 5;
    series.colors.step = 3;

    series.hiddenState.properties.opacity = 1;
    series.hiddenState.properties.endAngle = -90;
    series.hiddenState.properties.startAngle = -90;

    chart.numberFormatter.numberFormat = "#,###.##₺";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    //chart.legend = new am4charts.Legend();//Alt bilgi
    DownloadIconCustom(chart);
}

// Firma bazında bakım
function chart4_vehicleMain(data) {
    am4core.useTheme(am4themes_dark);
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart4", am4charts.PieChart3D);
    chart.hiddenState.properties.opacity = 0;
    chart.data = data;
    chart.depth = 20;

    var series = chart.series.push(new am4charts.PieSeries3D());
    series.dataFields.value = "Amount";
    series.dataFields.category = "SupplierName";

    series.hiddenState.properties.opacity = 1;
    series.hiddenState.properties.endAngle = -90;
    series.hiddenState.properties.startAngle = -90;

    chart.numberFormatter.numberFormat = "#,###.##₺";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    //chart.legend = new am4charts.Legend();//Alt bilgi
    DownloadIconCustom(chart);
}


function createSeries(field, name, chart, color) {
    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.name = name;
    series.dataFields.valueY = field;
    series.dataFields.categoryX = "Plate";
    series.sequencedInterpolation = true;
    series.columns.template.fill = am4core.color(color);
    series.stacked = true;

    series.columns.template.width = am4core.percent(80);
    series.tooltipText = "[bold]{name}[/]\n[font-size:14px]{categoryX}: {valueY} ₺";
    return series;
}

// Araç bazlı tekil tutar bakım
function chart5_vehicleMain(data) {
    am4core.useTheme(am4themes_dark);
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart5", am4charts.XYChart3D);
    chart.data = data;

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "Plate";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.minGridDistance = 30;
    categoryAxis.renderer.labels.template.horizontalCenter = "right";
    categoryAxis.renderer.labels.template.verticalCenter = "middle";
    categoryAxis.tooltip.disabled = false;
    categoryAxis.renderer.minHeight = 110;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Araç Bazlı Bakım/Onarım Detay";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.lineX.strokeOpacity = 0;
    chart.cursor.lineY.strokeOpacity = 0;

    var mostObjectLength = getKeysWithHighestValue(data);
    var colorScheme = [
        "#c8a2c8", "#660099", "#808080",
        "#54a0ff", "#ff7f00", "#964b00", "#a29bfe", "#dfe6e9",
        "#00b894", "#00cec9", "#0984e3", "#6c5ce7", "#ffeaa7",
        "#fab1a0", "#ff7675", "#fd79a8", "#fdcb6e", "#e17055",
        "#d63031", "#feca57", "#5f27cd", "#54a0ff", "#01a3a4"
    ];

    for (var i = 0; i < mostObjectLength - 1; i++) {
        var amount = "Amount" + i;
        var desc = "Tutar-" + (i + 1);
        createSeries(amount, desc, chart, colorScheme[i]);
    }

    ////standart veriler
    chart.numberFormatter.numberFormat = "#,###.##";
    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.scrollbarY = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}

//object list içindeki en yüksek length değerini bulur
function getKeysWithHighestValue(data) {
    var highVal = 0;
    for (var i = 0; i < data.length; i++) {
        var temp = Object.keys(data[i]).length;
        if (temp > highVal)
            highVal = temp;
    }
    return highVal;
}