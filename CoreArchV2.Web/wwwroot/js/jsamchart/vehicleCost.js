// Araç bazlı kiralama
function chart1_vehicleCost(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart1", am4charts.XYChart3D);
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
    valueAxis.title.text = "Araç Bazlı Kiralama Tutar";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Amount";
    series.dataFields.categoryX = "Plate";
    series.tooltipText = "Toplam Tutar: [bold]{valueY}[/] ₺";
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

    chart.numberFormatter.numberFormat = "#,###.##";

    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    //chart.legend = new am4charts.Legend();//Alt bilgi
    DownloadIconCustom(chart);
}

// Araç bazlı kiralama detay
function chart3_vehicleCost(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart3", am4charts.XYChart3D);
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
    valueAxis.title.text = "Araç Bazlı Kiralama Detay";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    //var mostObjectLength = getKeysWithHighestValue(data);
    var colorScheme = [
        "#FF9800", "#c0392b", "#bdc3c7", "#7f8c8d",
        "#55efc4", "#81ecec", "#74b9ff", "#a29bfe", "#dfe6e9",
        "#00b894", "#00cec9", "#0984e3", "#6c5ce7", "#ffeaa7",
        "#fab1a0", "#ff7675", "#fd79a8", "#fdcb6e", "#e17055",
        "#d63031", "#feca57", "#5f27cd", "#54a0ff", "#01a3a4"
    ];
    
    createSeries("Amount", "Kira Bedeli", chart, "#FFB74D");
    createSeries("ExtraAmount", "Extra Giderler", chart, colorScheme[10]);
    createSeries("ArventoAmount", "Diğer Giderler(Arvento ve Sim Kart)", chart, "#000000");

    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    //standart veriler
    chart.numberFormatter.numberFormat = "#,###.##";
    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}

// Kısa-uzun Dönem
function chart5_vehicleCost(data) {
    am4core.useTheme(am4themes_animated);
    // Themes end

    var chart = am4core.create("chart5", am4charts.PieChart);
    chart.hiddenState.properties.opacity = 0; // this creates initial fade-in

    chart.data = data;
    chart.radius = am4core.percent(70);
    chart.innerRadius = am4core.percent(40);
    chart.startAngle = 180;
    chart.endAngle = 360;

    var series = chart.series.push(new am4charts.PieSeries());
    series.dataFields.value = "Amount";
    series.dataFields.category = "RentTypeName";

    series.slices.template.cornerRadius = 10;
    series.slices.template.innerCornerRadius = 7;
    series.slices.template.draggable = true;
    series.slices.template.inert = true;
    series.alignLabels = false;

    series.hiddenState.properties.startAngle = 90;
    series.hiddenState.properties.endAngle = 90;

    chart.legend = new am4charts.Legend();




    //am4core.useTheme(am4themes_animated);
    //var chart = am4core.create("chart5", am4charts.PieChart3D);
    //chart.data = data
    //chart.innerRadius = am4core.percent(40);
    //chart.depth = 80;

    //var series = chart.series.push(new am4charts.PieSeries3D());
    //series.dataFields.value = "Amount";
    //series.dataFields.depthValue = "Amount";
    //series.dataFields.category = "RentTypeName";
    //series.slices.template.cornerRadius = 5;
    //series.colors.step = 3;

    //series.hiddenState.properties.opacity = 1;
    //series.hiddenState.properties.endAngle = -90;
    //series.hiddenState.properties.startAngle = -90;

    chart.numberFormatter.numberFormat = "#,###.##₺";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    //chart.legend = new am4charts.Legend();//Alt bilgi
    DownloadIconCustom(chart);
}

// Müdürlük bazında kiralama
function chart2_vehicleCost(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart2", am4charts.RadarChart);
    chart.data = data;
    chart.innerRadius = am4core.percent(40);
    chart.seriesContainer.zIndex = -10;
    chart.zoomOutButton.disabled = true;

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.dataFields.category = "UnitName";
    categoryAxis.renderer.minGridDistance = 60;
    categoryAxis.renderer.inversed = true;
    categoryAxis.renderer.labels.template.location = 0.5;
    categoryAxis.renderer.grid.template.strokeOpacity = 0.08;
    categoryAxis.renderer.tooltipLocation = 0.001;

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.min = 0;
    valueAxis.extraMax = 0.1;
    valueAxis.renderer.grid.template.strokeOpacity = 0.08;
    
    var series = chart.series.push(new am4charts.RadarColumnSeries());
    series.dataFields.categoryX = "UnitName";
    series.dataFields.valueY = "Amount";
    series.tooltipText = "{valueY.value} ₺"
    series.columns.template.strokeOpacity = 0;
    series.columns.template.radarColumn.cornerRadius = 5;
    series.columns.template.radarColumn.innerCornerRadius = 0;

    // as by default columns of the same series are of the same color, we add adapter which takes colors from chart.colors color set
    series.columns.template.adapter.add("fill", (fill, target) => {
        return chart.colors.getIndex(target.dataItem.index);
    });

    chart.cursor = new am4charts.RadarCursor();
    chart.cursor.behavior = "none";
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.fontFamily = "Times New Roman";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    chart.numberFormatter.numberFormat = "#,###.##₺";
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

    series.columns.template.width = am4core.percent(60);
    series.columns.template.tooltipText = "[bold]{name}[/]\n[font-size:14px]{categoryX}: {valueY} ₺";
    return series;
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

// Firma bazında yakıt
function chart4_vehicleCost(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart4", am4charts.PieChart3D);
    chart.data = data
    chart.depth = 80;
    chart.radius = am4core.percent(60);
    chart.innerRadius = am4core.percent(35);

    var series = chart.series.push(new am4charts.PieSeries3D());
    series.dataFields.value = "Amount";
    series.dataFields.depthValue = "Amount";
    series.dataFields.category = "RentFirmName";
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