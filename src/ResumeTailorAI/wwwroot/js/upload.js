$(function() {
    var uploadHandler = {
        init: function() {
            this.bindEvents();
        },

        bindEvents: function() {
            $("#btn-upload-resume").off("click").on("click", function() {
                uploadHandler.showUploadModal("resume");
            });

            $("#btn-upload-jd").off("click").on("click", function() {
                uploadHandler.showUploadModal("jd");
            });

            $("#btn-upload-file").off("click").on("click", function() {
                uploadHandler.uploadFile();
            });

            $("#file-input").off("change").on("change", function() {
                uploadHandler.validateFile();
            });
        },

        showUploadModal: function(type) {
            $("#upload-type").val(type);
            $("#upload-modal-title").text(type === "resume" ? "Upload Resume" : "Upload Job Description");
            $("#file-input").val("");
            $("#upload-progress").addClass("d-none");
            $("#upload-error").addClass("d-none").text("");
            $("#btn-upload-file").prop("disabled", false).text("Upload");
            
            var modalEl = document.getElementById("uploadModal");
            var modal = bootstrap.Modal.getInstance(modalEl);
            if (!modal) {
                modal = new bootstrap.Modal(modalEl);
            }
            modal.show();
        },

        validateFile: function() {
            var file = $("#file-input")[0].files[0];
            if (!file) {
                return true;
            }

            var maxSize = 10 * 1024 * 1024;
            if (file.size > maxSize) {
                this.showError("File size exceeds 10MB limit");
                return false;
            }

            var allowedExt = [".pdf", ".docx", ".md", ".txt"];
            var ext = "." + file.name.split(".").pop().toLowerCase();
            if (allowedExt.indexOf(ext) === -1) {
                this.showError("Unsupported file type. Allowed: PDF, DOCX, MD, TXT");
                return false;
            }

            return true;
        },

        uploadFile: function() {
            var file = $("#file-input")[0].files[0];
            if (!file) {
                this.showError("Please select a file");
                return;
            }

            if (!this.validateFile()) {
                return;
            }

            var type = $("#upload-type").val();
            var formData = new FormData();
            formData.append("file", file);

            $("#btn-upload-file").prop("disabled", true).text("Uploading...");
            $("#upload-progress").removeClass("d-none");
            $(".progress-bar").css("width", "0%").text("0%");

            var apiPath = type === "resume" ? "/api/resume/upload" : "/api/jd/upload";

            $.ajax({
                url: apiPath,
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                xhr: function() {
                    var xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener("progress", function(evt) {
                        if (evt.lengthComputable) {
                            var percent = Math.round((evt.loaded / evt.total) * 100);
                            $(".progress-bar").css("width", percent + "%").text(percent + "%");
                        }
                    }, false);
                    return xhr;
                },
                success: function(response) {
                    if (response.success) {
                        uploadHandler.onUploadSuccess(type, file.name, file.size);
                        var modal = bootstrap.Modal.getInstance(document.getElementById("uploadModal"));
                        if (modal) {
                            modal.hide();
                        }
                    } else {
                        uploadHandler.showError(response.message || "Upload failed");
                    }
                },
                error: function(xhr) {
                    var msg = "Upload failed";
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        msg = xhr.responseJSON.message;
                    }
                    uploadHandler.showError(msg);
                },
                complete: function() {
                    $("#btn-upload-file").prop("disabled", false).text("Upload");
                    $("#upload-progress").addClass("d-none");
                }
            });
        },

        onUploadSuccess: function(type, fileName, fileSize) {
            if (type === "resume") {
                $("#resume-status").text(fileName).removeClass("text-muted").addClass("text-success");
                window.resumeUploaded = true;
            } else {
                $("#jd-status").text(fileName).removeClass("text-muted").addClass("text-success");
                window.jdUploaded = true;
            }
            uploadHandler.updateAnalyzeButton();
        },

        updateAnalyzeButton: function() {
            var targetRole = $("#target-role").val();
            var canAnalyze = window.resumeUploaded && window.jdUploaded && targetRole;
            $("#btn-analyze").prop("disabled", !canAnalyze);
        },

        showError: function(message) {
            $("#upload-error").removeClass("d-none").text(message);
        }
    };

    window.uploadHandler = uploadHandler;
});