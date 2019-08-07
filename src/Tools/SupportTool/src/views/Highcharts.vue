<template>
  <v-card>
    <v-card-text>
      <v-layout wrap>
        <v-flex xs12 md6>
          <label>Sample Highcharts Data</label>
          <vue-highcharts :options="chartOptions" ref="lineCharts"></vue-highcharts>
        </v-flex>
      </v-layout>
    </v-card-text>
  </v-card>
</template>

<script>
import axios from "axios";
import VueHighcharts from "vue2-highcharts";
import ChartOptions from "./highcharts-options";

export default {
  name: "Highcharts",
  components: {
    VueHighcharts
  },
  data: () => ({
    chartOptions: ChartOptions,
    chartData: null
  }),
  mounted() {
    console.log("HighCharts::mounted()");
    this.appSettings = this.$store.state.appSettings;

    if (!this.$store.state.highChartsLoaded) {
      this.GetChartData();
    }

    if (this.$store.state.highChartsLoaded) {
      this.chartData = this.$store.state.highChartsData;
      this.RenderChart();
    }
  },
  created() {
    console.log("Highcharts::created()");
  },
  methods: {
    GetChartData() {
      console.log("GetChartData()");
      // let resp = await fetchGet("https://www.highcharts.com/samples/data/aapl-c.json");

      axios
        .get("https://www.highcharts.com/samples/data/aapl-c.json", {
          headers: {
            "Access-Control-Allow-Origin": "*"
          }
        })
        .then(result => {
          console.log("result", result);
          this.chartData = result.data;
          this.$store.dispatch("chartData", this.chartData);

          this.RenderChart();
        })
        .catch(error => {
          console.log("fetch-error", error);
          this.showProgress = false;
        });
    },
    RenderChart: function() {
      console.log("RenderChart()", this.chartData);

      let lineCharts = this.$refs.lineCharts;
      lineCharts.delegateMethod("showLoading", "Loading...");

      lineCharts.addSeries({
        name: "AAPL Stock Price",
        data: this.chartData,
        type: "area",
        threshold: null,
        tooltip: {
          valueDecimals: 2
        }
      });

      this.$store.dispatch("chartLoaded", true);
    }
  }
};
</script>