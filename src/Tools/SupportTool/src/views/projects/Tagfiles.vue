<template>
  <div>
    <h1>Tag File Upload</h1>
    <div>
      <v-alert
        type="success"
        v-model="uploaded"
        dismissible
        outlined
      >Tag Files Uploaded Successfully</v-alert>
      <v-alert
        type="error"
        v-model="uploadFailed"
        dismissible
        outlined
      >Tag Files Failed to Upload - See Developer Tools</v-alert>
      <v-card style="height: 400px;" v-if="uploading">
        <v-layout align-content-center justify-center fill-height wrap>
          <v-flex xs12 subtitle-1 text-center>Uploading Tag File: {{ currentFile.name }}</v-flex>
          <v-flex xs6>
            <v-progress-linear color="green accent-4" v-model="uploadProgress" rounded height="6"></v-progress-linear>
          </v-flex>
        </v-layout>
      </v-card>
    </div>

    <div v-if="!uploading">
      <p>
        <file-select
          v-model="files"
          :allow-multiple="true"
          prompt="Step 1: Select Tag Files"
          :disabled="uploading"
        ></file-select>
      </p>

      <!-- stage 2 - project UID -->
      <p>
        <v-card outlined :disabled="!files || uploading">
          <v-list-item two-line>
            <v-list-item-content>
              <v-list-item-title class="overline mb-4">Step 2: Enter Project UID</v-list-item-title>
              <v-text-field
                v-model="projectUid"
                :rules="projectUidRules"
                label="Project UID"
                required
              ></v-text-field>
            </v-list-item-content>
          </v-list-item>
        </v-card>
      </p>

      <!-- stage 3 - Upload files -->
      <p>
        <v-card outlined :disabled="!files || !projectUid || uploading">
          <v-list-item two-line>
            <v-list-item-content>
              <v-list-item-title class="overline mb-4">Step 3: Upload Tag Files</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
          <v-card-actions>
            <v-btn class="select-button ma-2" color="primary" v-on:click="uploadTagFiles">Upload</v-btn>
          </v-card-actions>
        </v-card>
      </p>
    </div>
  </div>
</template>

<script>
import FileSelect from "../../components/FileSelect.vue";
import Consts from "../../const.js";
import axios from "axios";
var urljoin = require("url-join");

export default {
  name: "tagfiles",
  components: {
    FileSelect
  },
  data() {
    return {
      uploaded: false,
      uploadFailed: false,
      uploading: false,
      files: null,
      currentFile: null,
      uploadProgress: 0,
      projectUid: null,
      projectUidRules: [
        v => !!v || "A Project UID must be provided.",
        v => Consts.GUID_REGEX.test(v) || "Project UID must be a valid UID."
      ]
    };
  },
  methods: {
    uploadTagFiles: function(event) {
      var self = this;
      this.uploading = true;
      console.log("Uploading file", this.files);
      this.base64Encode(file).then(encoded => {});
    }
  }
};
</script>