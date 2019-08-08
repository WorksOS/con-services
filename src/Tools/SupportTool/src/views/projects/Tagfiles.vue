<template>
  <div>
    <h1>Tag File Upload</h1>

    <p>
      <v-alert
        v-for="(value, key) in uploadResults"
        v-bind:key="key"
        v-bind:type="value.type"
        v-model="uploadResults[key].visible"
        dark
        outlined
      >
        <div class="title">{{key}}</div>
        <div v-if="value.inprogress">Currently Uploading</div>
        <div v-else>{{ value.message }} ({{value.code}})</div>
      </v-alert>
      <v-spacer></v-spacer>
      <v-btn block color="primary" dark v-if="uploading" @click="reset">Done</v-btn>
    </p>



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
      uploading: false,
      files: null,
      projectUid: null,
      uploadResults: {},
      projectUidRules: [
        v => !!v || "A Project UID must be provided.",
        v => Consts.GUID_REGEX.test(v) || "Project UID must be a valid UID."
      ]
    };
  },
  methods: {
    reset: function() {
        console.log("Resetting");
        this.uploading = false;
        this.uploadResults = {};
    },
    uploadTagFiles: function(event) {
      var self = this;
      this.uploading = true;
      // Get the URL
      const url = urljoin(
        config.VUE_APP_TREX_MUTABLE_GATEWAY_URL,
        "/api/v2/tagfiles"
      );

      for (var i = 0; i < self.files.length; i++) {
        var file = this.files[i];
        this.uploadResults[file.name] = {
          success: false,
          inprogress: true,
          visible: true,
          type: "info"
        };
        
        this.base64Encode(file).then(res => {
            
          var f = res.file; // Get a reference to the file
          var encoded = res.encoded; // Get the base64 encoded data

          // Generate the payload for each tag file
          var payload = {
            projectUid: this.projectUid,
            fileName: f.name,
            data: encoded
          };

          axios
            .post(url, payload, {
              headers: {
                "Content-Type": "application/json"
              }
            })
            .then(result => {
              console.log("result", f.name, result.data);
              self.uploadResults[f.name].success = true;
              self.uploadResults[f.name].inprogress = false;
              self.uploadResults[f.name].message = result.data.Message || null;
              self.uploadResults[f.name].code = result.data.Code || -1;
              self.uploadResults[f.name].type = result.data.Code == 0 ? "success" : "warning";
              self.$forceUpdate();
            })
            .catch(error => {
              console.log("fetch-error", error);
              var message = null;
              if (
                error.response &&
                error.response.data &&
                error.response.data.Message
              )
                message = error.response.data.Message;

              self.uploadResults[f.name].success = false;
              self.uploadResults[f.name].inprogress = false;
              self.uploadResults[f.name].message = result.data.Message || null;
              self.uploadResults[f.name].type = "error";
              self.$forceUpdate();
            });
        });
      }
    }
  }
};
</script>