// Kullanıcı bazlı km
function chart5_vehicleTrip(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart5", am4charts.XYChart3D);
    chart.data = data;

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "UserNameSurname";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.minGridDistance = 30;
    categoryAxis.renderer.labels.template.horizontalCenter = "right";
    categoryAxis.renderer.labels.template.verticalCenter = "middle";
    categoryAxis.tooltip.disabled = false;
    categoryAxis.renderer.minHeight = 110;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Kullanıcı Bazlı Km";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    // Create series
    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "TotalKm";
    series.dataFields.categoryX = "UserNameSurname";
    series.tooltipText = "Toplam: [bold]{valueY}[/] Km";
    series.columns.template.fillOpacity = .8;

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.lineX.strokeOpacity = 0;
    chart.cursor.lineY.strokeOpacity = 0;

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

// Araç bazlı görev adedi
function chart6_vehicleTrip(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart6", am4charts.XYChart3D);
    chart.data = data;

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "UserNameSurname";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.minGridDistance = 30;
    categoryAxis.renderer.labels.template.horizontalCenter = "right";
    categoryAxis.renderer.labels.template.verticalCenter = "middle";
    categoryAxis.tooltip.disabled = false;
    categoryAxis.renderer.minHeight = 110;
    categoryAxis.renderer.labels.template.rotation = -50;
    categoryAxis.renderer.labels.template.fontFamily = "robotic";

    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Kullanıcı Bazlı Görev Sayısı";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    // Create series
    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Count";
    series.dataFields.categoryX = "UserNameSurname";
    series.tooltipText = "Görev Sayısı: [bold]{valueY}[/] Adet";
    series.columns.template.fillOpacity = .8;

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.lineX.strokeOpacity = 0;
    chart.cursor.lineY.strokeOpacity = 0;

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


// Araç bazlı km
function chart1_vehicleTrip(data) {
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
    valueAxis.title.text = "Araç Bazlı Km";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    // Create series
    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "TotalKm";
    series.dataFields.categoryX = "Plate";
    series.tooltipText = "Toplam: [bold]{valueY}[/] Km";
    series.columns.template.fillOpacity = .8;
    series.columns.template.fill = am4core.color("#039BE5");

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.lineX.strokeOpacity = 0;
    chart.cursor.lineY.strokeOpacity = 0;

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

//Araç bazlı örev sayısı
function chart4_vehicleTrip(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart4", am4charts.XYChart3D);
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
    valueAxis.title.text = "Araç Bazlı Görev Sayısı";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    // Create series
    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Count";
    series.dataFields.categoryX = "Plate";
    series.tooltipText = "Toplam: [bold]{valueY}[/] Adet";
    series.columns.template.fillOpacity = .8;
    series.columns.template.fill = am4core.color("#FFB74D");

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.lineX.strokeOpacity = 0;
    chart.cursor.lineY.strokeOpacity = 0;

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

// Müdürlük bazında yakıt
function chart2_vehicleTrip(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart2", am4charts.PieChart3D);
    chart.data = data
    chart.radius = am4core.percent(60);
    chart.innerRadius = am4core.percent(35);
    chart.depth = 80;

    var series = chart.series.push(new am4charts.PieSeries3D());
    series.dataFields.value = "Count";
    series.dataFields.depthValue = "Count";
    series.dataFields.category = "UnitName";
    series.slices.template.cornerRadius = 5;
    series.colors.step = 3;

    series.hiddenState.properties.opacity = 1;
    series.hiddenState.properties.endAngle = -90;
    series.hiddenState.properties.startAngle = -90;

    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    //chart.legend = new am4charts.Legend();//Alt bilgi
    DownloadIconCustom(chart);
}

// Ay bazlı yakıt
function chart3_vehicleTrip(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart3", am4charts.XYChart);
    chart.data = data

    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.ticks.template.disabled = true;
    categoryAxis.renderer.line.opacity = 0;
    categoryAxis.renderer.grid.template.disabled = true;
    categoryAxis.renderer.minGridDistance = 40;
    categoryAxis.dataFields.category = "TransactionDate";
    categoryAxis.startLocation = 0.4;
    categoryAxis.endLocation = 0.6;


    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.tooltip.disabled = true;
    valueAxis.renderer.line.opacity = 0;
    valueAxis.renderer.ticks.template.disabled = true;
    valueAxis.min = 0;

    var lineSeries = chart.series.push(new am4charts.LineSeries());
    lineSeries.dataFields.categoryX = "TransactionDate";
    lineSeries.dataFields.valueY = "DebitPlateCount";
    lineSeries.tooltipText = "Toplam Görev Sayısı: [bold]{valueY}[/] Adet"
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
    valueAxis.title.text = "Ay Bazlı Görev";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    //standart veriler
    chart.cursor = new am4charts.XYCursor();//mouse ile zoom yapar
    chart.cursor.lineX.disabled = true;
    chart.cursor.lineY.disabled = true;

    chart.scrollbarX = new am4core.Scrollbar();  //scrollBar
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
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
    series.tooltipText = "[bold]{name}[/]\n[font-size:14px]{categoryX}: {valueY} adet";
    return series;
}