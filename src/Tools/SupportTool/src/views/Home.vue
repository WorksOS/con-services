<template>
  <v-app id="app">
    <v-navigation-drawer
      :permanent="appSettings.primaryDrawer.type === 'permanent'"
      :temporary="appSettings.primaryDrawer.type === 'temporary'"
      :clipped="appSettings.primaryDrawer.clipped"
      :floating="appSettings.primaryDrawer.floating"
      :mini-variant="appSettings.primaryDrawer.mini"
      overflow
      app
    >
      <v-list dense>
        <v-list-group v-for="(rootItem, i) in menuItems" :key="i" no-action :value="true">
          <template v-slot:activator>
            <v-list-item-icon>
              <v-icon v-text="rootItem.icon"></v-icon>
            </v-list-item-icon>
            <v-list-item-title>{{rootItem.text}}</v-list-item-title>
          </template>

          <v-list-item v-for="(item, i) in rootItem.items" :key="i" :to="item.link">
            <v-list-item-title v-text="item.text"></v-list-item-title>
            <v-list-item-icon>
              <v-icon v-text="item.icon"></v-icon>
            </v-list-item-icon>
          </v-list-item>
        </v-list-group>
      </v-list>
    </v-navigation-drawer>

    <v-app-bar :clipped-left="appSettings.primaryDrawer.clipped" app>
      <v-app-bar-nav-icon
        v-if="appSettings.primaryDrawer.type !== 'permanent'"
        @click.stop="appSettings.primaryDrawer.model = !appSettings.primaryDrawer.model"
      ></v-app-bar-nav-icon>
      <v-toolbar-title>TRex Support Tool</v-toolbar-title>
    </v-app-bar>

    <v-content>
      <v-container fluid>
        <v-layout align-center justify-center>
          <v-flex xs12>
            <router-view></router-view>
          </v-flex>
        </v-layout>
      </v-container>
    </v-content>

    <v-footer :inset="appSettings.footer.inset" app>
      <span class="px-3">&copy; {{ new Date().getFullYear() }}</span>
    </v-footer>
  </v-app>
</template>

<script>
export default {
  data: () => ({
    appSettings: null,
    menuItems: {
      projects: {
        icon: "mdi-book-open",
        text: "Projects",
        items: [
          {
            icon: "mdi-apps-box",
            text: "Process Tag Files",
            link: "/project/tagfiles"
          },
          {
            icon: "mdi-axis-y-arrow",
            text: "Import DC File",
            link: "/project/coordsystem"
          }
        ]
      }
    }
  }),
  created() {
    this.appSettings = this.$store.state.appSettings;
  }
};
</script>
