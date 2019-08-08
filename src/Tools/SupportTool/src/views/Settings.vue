<template>
  <v-card>
    <v-card-text>
      <v-layout wrap>
        <v-flex xs12 md6>
          <span>Scheme</span>
          <v-switch v-model="$vuetify.theme.dark" primary label="Dark"></v-switch>
        </v-flex>
        <v-flex xs12 md6>
          <span>Drawer</span>
          <v-radio-group v-model="appSettings.primaryDrawer.type" column>
            <v-radio
              v-for="drawer in appSettings.drawers"
              :key="drawer"
              :label="drawer"
              :value="drawer.toLowerCase()"
              primary
            ></v-radio>
          </v-radio-group>
          <v-switch v-model="appSettings.primaryDrawer.clipped" label="Clipped" primary></v-switch>
          <v-switch v-model="appSettings.primaryDrawer.floating" label="Floating" primary></v-switch>
          <v-switch v-model="appSettings.primaryDrawer.mini" label="Mini" primary></v-switch>
        </v-flex>
        <v-flex xs12 md6>
          <span>Footer</span>
          <v-switch v-model="appSettings.footer.inset" label="Inset" primary></v-switch>
        </v-flex>
      </v-layout>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn>Cancel</v-btn>
      <v-btn color="primary">Submit</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
export default {
  name: "settings",
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