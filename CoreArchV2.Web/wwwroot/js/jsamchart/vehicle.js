//chart - 1 Araç sayıları 
function chart1(data) {
    var arr = [];
    arr.push({ key: "Kullanılmayan Araç(Havuz-Servis)", value: data.EmptyVehicle.length });
    arr.push({ key: "Kiralık Araç", value: data.RentVehicle.length });
    arr.push({ key: "Mülkiyet Araç", value: data.FixVehicle.length });
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart1", am4charts.PieChart3D);
    chart.data = arr;

    chart.radius = am4core.percent(60);
    chart.innerRadius = am4core.percent(35);

    var series = chart.series.push(new am4charts.PieSeries3D());
    series.dataFields.value = "value";
    series.dataFields.category = "key";

    // this creates initial animation
    series.hiddenState.properties.opacity = 1;
    series.hiddenState.properties.endAngle = -90;
    series.hiddenState.properties.startAngle = -90;

    chart.legend = new am4charts.Legend();
    chart.fontFamily = "Times New Roman";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}


//chart - 2 Hangi firmada kaç araç var
function chart2(data) {
    am4core.useTheme(am4themes_kelly);
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart2", am4charts.XYChart3D);
    chart.colors.saturation = 0.4;

    chart.data = data;

    var categoryAxis = chart.yAxes.push(new am4charts.CategoryAxis());
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.dataFields.category = "Name";
    categoryAxis.renderer.minGridDistance = 20;
    categoryAxis.renderer.labels.template.rotation = -15;

    var valueAxis = chart.xAxes.push(new am4charts.ValueAxis());
    valueAxis.renderer.maxLabelPosition = 0.98;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.categoryY = "Name";
    series.dataFields.valueX = "Count";
    series.tooltipText = "Araç sayısı: {valueX.value} Adet";
    series.sequencedInterpolation = true;
    series.defaultState.transitionDuration = 1000;
    series.sequencedInterpolationDelay = 100;
    series.columns.template.strokeOpacity = 1;

    series.columns.template.adapter.add("fill", (fill, target) => {
        return chart.colors.getIndex(target.dataItem.index);
    });

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.behavior = "zoomY";
    chart.fontSize = 11;

    chart.fontFamily = "Times New Roman"
    var columnTemplate = series.columns.template;
    columnTemplate.fillOpacity = 1;
    columnTemplate.strokeOpacity = 0;
    columnTemplate.fill = am4core.color("#FFA726");
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}

//Müdürlük bazında araç sayısı
function chart3(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart3", am4charts.RadarChart);
    chart.data = data;
    chart.innerRadius = am4core.percent(40);

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.dataFields.category = "UnitName";
    categoryAxis.renderer.minGridDistance = 60;
    categoryAxis.renderer.inversed = true;
    categoryAxis.renderer.labels.template.location = 0.5;
    categoryAxis.renderer.grid.template.strokeOpacity = 0.08;

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.min = 0;
    valueAxis.extraMax = 0.1;
    valueAxis.renderer.grid.template.strokeOpacity = 0.08;

    chart.seriesContainer.zIndex = -10;

    var series = chart.series.push(new am4charts.RadarColumnSeries());
    series.dataFields.categoryX = "UnitName";
    series.dataFields.valueY = "Count";
    series.tooltipText = "{valueY.value} Adet"
    series.columns.template.strokeOpacity = 0;
    series.columns.template.radarColumn.cornerRadius = 5;
    series.columns.template.radarColumn.innerCornerRadius = 0;
    chart.zoomOutButton.disabled = true;

    categoryAxis.renderer.tooltipLocation = 0.001;

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
    DownloadIconCustom(chart);
}

//Kullanım tipine göre araç sayısı
function chart4(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart4", am4charts.PieChart);
    chart.hiddenState.properties.opacity = 0;

    chart.data = data;
    chart.radius = am4core.percent(50);
    chart.innerRadius = am4core.percent(35);
    chart.startAngle = 180;
    chart.endAngle = 360;

    var series = chart.series.push(new am4charts.PieSeries());
    series.dataFields.value = "Count";
    series.dataFields.category = "Name";
    //series.labels.template.text = "{category}";

    series.slices.template.cornerRadius = 10;
    series.slices.template.innerCornerRadius = 7;
    series.slices.template.inert = true;
    series.alignLabels = false;
    series.labels.template.maxWidth = 130;
    series.labels.template.wrap = true;

    series.hiddenState.properties.startAngle = 90;
    series.hiddenState.properties.endAngle = 90;

    chart.legend = new am4charts.Legend();
    chart.fontFamily = "Times New Roman";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}

function chart5(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart5", am4charts.XYChart3D);
    chart.data = data;

    // Create axes
    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "VehicleModelYear";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.minGridDistance = 30;
    categoryAxis.renderer.labels.template.horizontalCenter = "right";
    categoryAxis.renderer.labels.template.verticalCenter = "middle";
    categoryAxis.tooltip.disabled = false;
    categoryAxis.renderer.minHeight = 110;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Araç Model Yıl";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Count";
    series.dataFields.categoryX = "VehicleModelYear";
    series.tooltipText = "Toplam: [bold]{valueY}[/] Adet";
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

//Kullanım tipine göre araç sayısı
function chart6(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart6", am4charts.PieChart);
    chart.hiddenState.properties.opacity = 0;

    chart.data = data;
    chart.radius = am4core.percent(50);
    chart.innerRadius = am4core.percent(35);
    chart.startAngle = 180;
    chart.endAngle = 360;

    var series = chart.series.push(new am4charts.PieSeries());
    series.dataFields.value = "Amount";
    series.dataFields.category = "VehicleModelYear";
    //series.labels.template.text = "{category}";

    series.slices.template.cornerRadius = 10;
    series.slices.template.innerCornerRadius = 7;
    series.slices.template.inert = true;
    series.alignLabels = false;
    series.labels.template.maxWidth = 130;
    series.labels.template.wrap = true;

    series.hiddenState.properties.startAngle = 90;
    series.hiddenState.properties.endAngle = 90;

    chart.legend = new am4charts.Legend();
    chart.fontFamily = "Times New Roman";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}