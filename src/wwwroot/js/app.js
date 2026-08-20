var ResumeTailorApp = (function() {
    var sessionId = null;
    var resumeUploaded = false;
    var jdUploaded = false;
    var targetRole = "";

    var init = function() {
        bindEvents();
        checkSession();
    };

    var bindEvents = function() {
        $("#btn-upload-resume").click(function() {
            showUploadModal("resume");
        });

        $("#btn-upload-jd").click(function() {
            showUploadModal("jd");
        });

        $("#btn-analyze").click(function() {
            startAnalysis();
        });

        $("#btn-upload-file").click(function() {
            uploadFile();
        });

        $("#file-input").change(function() {
            validateFile();
        });
    };

    var showUploadModal = function(type) {
        $("#upload-type").val(type);
        $("#upload-modal-title").text(type === "resume" ? "Upload Resume" : "Upload Job Description");
        $("#file-input").val("");
        $("#upload-progress").addClass("d-none");
        $("#upload-error").addClass("d-none").text("");
        $("#upload-form")[0].reset();
        var modal = new bootstrap.Modal(document.getElementById("uploadModal"));
        modal.show();
    };

    var validateFile = function() {
        var file = $("#file-input")[0].files[0];
        if (!file) return;

        var maxSize = 10 * 1024 * 1024;
        if (file.size > maxSize) {
            showError("File size exceeds 10MB limit");
            return false;
        }

        var allowedExt = [".pdf", ".docx", ".md", ".txt"];
        var ext = "." + file.name.split(".").pop().toLowerCase();
        if (allowedExt.indexOf(ext) === -1) {
            showError("Unsupported file type. Allowed: PDF, DOCX, MD, TXT");
            return false;
        }

        return true;
    };

    var uploadFile = function() {
        var file = $("#file-input")[0].files[0];
        if (!file) {
            showError("Please select a file");
            return;
        }

        if (!validateFile()) return;

        var type = $("#upload-type").val();
        var formData = new FormData();
        formData.append("file", file);

        $("#btn-upload-file").prop("disabled", true).text("Uploading...");
        $("#upload-progress").removeClass("d-none");

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
                    $("#upload-error").addClass("d-none");
                    if (type === "resume") {
                        resumeUploaded = true;
                        $("#resume-status").text(file.name).removeClass("text-muted").addClass("text-success");
                    } else {
                        jdUploaded = true;
                        $("#jd-status").text(file.name).removeClass("text-muted").addClass("text-success");
                    }
                    updateAnalyzeButton();
                    var modal = bootstrap.Modal.getInstance(document.getElementById("uploadModal"));
                    modal.hide();
                } else {
                    showError(response.message || "Upload failed");
                }
            },
            error: function(xhr) {
                var msg = "Upload failed";
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg = xhr.responseJSON.message;
                }
                showError(msg);
            },
            complete: function() {
                $("#btn-upload-file").prop("disabled", false).text("Upload");
                $("#upload-progress").addClass("d-none");
            }
        });
    };

    var showError = function(message) {
        $("#upload-error").removeClass("d-none").text(message);
    };

    var checkSession = function() {
        $.get("/api/session/status", function(response) {
            if (response.success && response.data) {
                $("#session-status").text("Active").removeClass("text-warning").addClass("text-success");
            }
        }).fail(function() {
            startSession();
        });
    };

    var startSession = function() {
        targetRole = $("#target-role").val();
        if (!targetRole) {
            alert("Please enter a target role");
            return;
        }

        $.post("/api/session/start", { targetRole: targetRole })
            .done(function(response) {
                if (response.success) {
                    sessionId = response.data.sessionId;
                    $("#session-status").text("Active").removeClass("text-warning").addClass("text-success");
                }
            })
            .fail(function() {
                alert("Failed to start session");
            });
    };

    var updateAnalyzeButton = function() {
        targetRole = $("#target-role").val();
        $("#btn-analyze").prop("disabled", !(resumeUploaded && jdUploaded && targetRole));
    };

    $("#target-role").on("input", function() {
        targetRole = $(this).val();
        updateAnalyzeButton();
    });

    var startAnalysis = function() {
        targetRole = $("#target-role").val();
        
        if (!targetRole) {
            if (!sessionId) {
                startSession();
                return;
            }
            if (!resumeUploaded || !jdUploaded) {
                return;
            }
        }

        if (!resumeUploaded || !jdUploaded) {
            return;
        }

        $.post("/api/session/start", { targetRole: targetRole })
            .done(function(response) {
                if (response.success) {
                    sessionId = response.data.sessionId;
                    $("#session-status").text("Analyzing...").removeClass("text-success").addClass("text-info");
                    
                    $.post("/api/analysis/start")
                        .done(function(resp) {
                            if (resp.success) {
                                window.location.href = "/resume/result";
                            } else {
                                $("#session-status").text("Failed").addClass("text-danger");
                                alert("Analysis failed: " + resp.message);
                            }
                        })
                        .fail(function() {
                            $("#session-status").text("Failed").addClass("text-danger");
                            alert("Analysis request failed");
                        });
                }
            })
            .fail(function() {
                alert("Failed to start session");
            });
    };

    var getSessionId = function() {
        return sessionId;
    };

    var isResumeUploaded = function() {
        return resumeUploaded;
    };

    var isJdUploaded = function() {
        return jdUploaded;
    };

    return {
        init: init,
        getSessionId: getSessionId,
        startAnalysis: startAnalysis,
        isResumeUploaded: isResumeUploaded,
        isJdUploaded: isJdUploaded
    };
})();

$(document).ready(function() {
    ResumeTailorApp.init();
});