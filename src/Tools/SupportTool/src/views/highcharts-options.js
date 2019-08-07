var chartOptions = {
  chart: {
    height: 400
  },
  title: {
    text: "Highstock Responsive Chart"
  },
  rangeSelector: {
    selected: 1
  },
  responsive: {
    rules: [{
      condition: {
        maxWidth: 500
      },
      chartOptions: {
        chart: {
          height: 300
        },
        subtitle: {
          text: null
        },
        navigator: {
          enabled: false
        }
      }
    }]
  },
  series: []
}

export default chartOptions;