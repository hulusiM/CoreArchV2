// Ay bazlı yakıt
function chart1_vehicleFuel(data) {
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


    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.tooltip.disabled = true;
    valueAxis.renderer.line.opacity = 0;
    valueAxis.renderer.ticks.template.disabled = true;
    valueAxis.min = 0;

    var lineSeries = chart.series.push(new am4charts.LineSeries());
    lineSeries.dataFields.categoryX = "FuelDate2";
    lineSeries.dataFields.valueY = "DiscountAmount";
    lineSeries.tooltipText = "Toplam İskontolu Tutar: [bold]{valueY}[/] ₺"
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
    valueAxis.title.text = "Ay Bazlı Yakıt Tutar";
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

// Müdürlük bazında yakıt
function chart2_vehicleFuel(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart2", am4charts.PieChart3D);
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

// Araç bazlı yakıt tutar
function chart3_vehicleFuel(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart3", am4charts.XYChart3D);
    chart.data = data;

    // Create axes
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
    valueAxis.title.text = "Araç Bazlı Yakıt Tutar";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Amount";
    series.dataFields.categoryX = "Plate";
    series.tooltipText = "İskontolu Tutar: [bold]{valueY}[/] ₺";
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

    chart.exporting.menu = new am4core.ExportMenu();//export bilgileri
    DownloadIconCustom(chart);
}

// Araç bazlı yakıt litre
function chart6_vehicleFuel(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart6", am4charts.XYChart3D);
    chart.data = data;

    // Create axes
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
    valueAxis.title.text = "Araç Bazlı Yakıt Litre";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Liter";
    series.dataFields.categoryX = "Plate";
    series.tooltipText = "Toplam Litre: [bold]{valueY}[/] Lt";
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

// Araç bazlı km başına tutar
function chart4_vehicleFuel(data, isAllVehicle) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart4", am4charts.XYChart3D);
    chart.data = data;

    // Create axes
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
    valueAxis.title.text = "Araç Km Bazlı Tutar";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "KmSpend";
    series.dataFields.categoryX = "Plate";
    series.columns.template.fillOpacity = .8;
    series.tooltipText = "TL/KM : [bold]{valueY}[/] ₺"
    series.columns.template.fill = am4core.color("#FFB74D");

    if ( data != null && data.length > 0 && data[0].AverageKmAmount > 0 && !isAllVehicle) { //Tüm araçlarda pasif
        var lineSeries = chart.series.push(new am4charts.LineSeries());
        lineSeries.name = "Ortalama Yakıt Tutarı";
        lineSeries.dataFields.valueY = "AverageKmAmount";
        lineSeries.dataFields.categoryX = "Plate";

        lineSeries.stroke = am4core.color("#fdd400");
        lineSeries.strokeWidth = 3;
        lineSeries.propertyFields.strokeDasharray = "lineDash";
        lineSeries.tooltip.label.textAlign = "middle";

        var bullet = lineSeries.bullets.push(new am4charts.Bullet());
        bullet.fill = am4core.color("#fdd400"); // tooltips grab fill from parent by default
        //bullet.tooltipText = "{name} : {valueY} ₺"
        series.tooltipText = "Plaka: [bold]{categoryX}[/] \n TL/KM: [bold]{KmSpend} ₺[/] \n Ortalama Yakıt Tutarı: [bold]{AverageKmAmount} ₺[/]";
        var circle = bullet.createChild(am4core.Circle);
        circle.radius = 4;
        circle.fill = am4core.color("#fff");
        circle.strokeWidth = 3;

    }

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

    chart.numberFormatter.numberFormat = "#,###.##";

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

// Araç bazlı km başına litre
function chart7_vehicleFuel(data, isAllVehicle) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart7", am4charts.XYChart3D);
    chart.data = data;

    // Create axes
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
    valueAxis.title.text = "Araç Km Bazlı Litre";
    valueAxis.title.fontWeight = "bold";
    valueAxis.min = 0;

    var series = chart.series.push(new am4charts.ColumnSeries3D());
    series.dataFields.valueY = "Liter";
    series.dataFields.categoryX = "Plate";
    series.columns.template.fillOpacity = .8;
    series.tooltipText = "Lt/Km : [bold]{valueY}[/] Lt"
    series.columns.template.fill = am4core.color("#FFB74D");

    if (data != null && data.length > 0 && data[0].AverageKmAmount > 0 && !isAllVehicle) { //Tüm araçlarda pasif
        var lineSeries = chart.series.push(new am4charts.LineSeries());
        lineSeries.name = "Ortalama Litre";
        lineSeries.dataFields.valueY = "AverageLiter";
        lineSeries.dataFields.categoryX = "Plate";

        lineSeries.stroke = am4core.color("#fdd400");
        lineSeries.strokeWidth = 3;
        lineSeries.propertyFields.strokeDasharray = "lineDash";
        lineSeries.tooltip.label.textAlign = "middle";

        var bullet = lineSeries.bullets.push(new am4charts.Bullet());
        bullet.fill = am4core.color("#fdd400"); // tooltips grab fill from parent by default
        //bullet.tooltipText = "{name} : {valueY} ₺"
        series.tooltipText = "Plaka: [bold]{categoryX}[/] \n Lt/Km: [bold]{Liter} Lt[/] \n Ortalama Litre: [bold]{AverageLiter} Lt[/]";
        var circle = bullet.createChild(am4core.Circle);
        circle.radius = 4;
        circle.fill = am4core.color("#fff");
        circle.strokeWidth = 3;

    }

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

    chart.numberFormatter.numberFormat = "#,###.##";

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


//Tedarikçi firma yakıt tutar
function chart5_vehicleFuel(data) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chart5", am4charts.PieChart);
    chart.hiddenState.properties.opacity = 0;

    chart.data = data;
    chart.radius = am4core.percent(70);
    chart.innerRadius = am4core.percent(40);
    chart.startAngle = 180;
    chart.endAngle = 360;

    var series = chart.series.push(new am4charts.PieSeries());
    series.dataFields.value = "Amount";
    series.dataFields.category = "SupplierName";

    series.slices.template.cornerRadius = 10;
    series.slices.template.innerCornerRadius = 7;
    series.slices.template.inert = true;
    series.alignLabels = false;

    series.hiddenState.properties.startAngle = 90;
    series.hiddenState.properties.endAngle = 90;

    chart.legend = new am4charts.Legend();
    chart.fontFamily = "Times New Roman";
    chart.exporting.menu = new am4core.ExportMenu();//excel,print
    DownloadIconCustom(chart);
}