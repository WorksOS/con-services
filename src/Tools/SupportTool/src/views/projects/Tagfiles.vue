<template>
    <div>
        <h1>Tag File Upload</h1>
    </div>
</template>

<script>
export default {
  name: "tagfiles",
  data: () => ({
    appSettings: null
  }),
  created() {
    this.appSettings = this.$store.state.appSettings;
    console.log('appSettings', this.$store.state.appSettings);
  },
  methods: {
    // setDarkTheme() {
    //   this.$store.commit("dark") = !this.$store.state.dark;
    // }
  }
};
</script>