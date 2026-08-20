$(function() {
    var approvalHandler = {
        init: function() {
            this.bindEvents();
            this.loadPreview();
        },

        bindEvents: function() {
            $("#btn-approve").off("click").on("click", function() {
                approvalHandler.approveDocuments();
            });

            $("#btn-reject").off("click").on("click", function() {
                approvalHandler.rejectDocuments();
            });

            $("#btn-download-resume").off("click").on("click", function() {
                approvalHandler.downloadResume();
            });

            $("#btn-download-cover-letter").off("click").on("click", function() {
                approvalHandler.downloadCoverLetter();
            });
        },

        loadPreview: function() {
            $.get("/api/tailoring/preview", function(response) {
                if (response.success && response.data) {
                    approvalHandler.renderResult(response.data);
                } else {
                    $("#preview-content").html("<div class=\"alert alert-warning\">No analysis results available. Please complete analysis first.</div>");
                }
            }).fail(function() {
                $("#preview-content").html("<div class=\"alert alert-danger\">Failed to load analysis results.</div>");
            });
        },

        renderResult: function(data) {
            var html = "";
            
            html += "<div class=\"card mb-4\">";
            html += "<div class=\"card-header bg-primary text-white\">";
            html += "<h4 class=\"mb-0\">Resume Tailoring Summary</h4>";
            html += "</div>";
            html += "<div class=\"card-body\">";

            html += "<div class=\"text-center mb-4\">";
            html += "<div class=\"match-score text-primary\">" + Math.round(data.overallMatchScore) + "%</div>";
            html += "<p class=\"text-muted\">Overall Fit Score</p>";
            html += "</div>";

            html += "<div class=\"row\">";
            html += "<div class=\"col-md-6\">";
            html += "<h5>Matched Skills</h5>";
            html += "<div class=\"mb-3\">";
            data.matchedSkills.forEach(function(skill) {
                html += "<span class=\"badge bg-success me-1 mb-1\">" + skill + "</span>";
            });
            html += "</div>";
            html += "</div>";

            html += "<div class=\"col-md-6\">";
            html += "<h5>Missing Skills</h5>";
            html += "<div class=\"mb-3\">";
            data.missingSkills.forEach(function(skill) {
                html += "<span class=\"badge bg-danger me-1 mb-1\">" + skill.skill + "</span>";
            });
            html += "</div>";
            html += "</div>";
            html += "</div>";

            if (data.partialMatches && data.partialMatches.length > 0) {
                html += "<h5 class=\"mt-3\">Partial Matches</h5>";
                html += "<ul class=\"list-group mb-3\">";
                data.partialMatches.forEach(function(pm) {
                    html += "<li class=\"list-group-item\"><strong>" + pm.skill + "</strong> - " + pm.gap + "</li>";
                });
                html += "</ul>";
            }

            var guardrailClass = data.guardrail.status === "PASS" ? "alert-success" : "alert-danger";
            html += "<div class=\"alert " + guardrailClass + "\">";
            html += "<strong>Guardrail: " + data.guardrail.status + "</strong>";
            if (data.guardrail.unsupportedClaims && data.guardrail.unsupportedClaims.length > 0) {
                html += "<ul class=\"mb-0 mt-2\">";
                data.guardrail.unsupportedClaims.forEach(function(claim) {
                    html += "<li>" + claim.claim + " - " + claim.reason + "</li>";
                });
                html += "</ul>";
            } else {
                html += "<p class=\"mb-0\">All claims validated against original resume.</p>";
            }
            html += "</div>";

            if (data.atsAnalysis) {
                html += "<h5>ATS Analysis</h5>";
                html += "<p><strong>ATS Score:</strong> " + data.atsAnalysis.atsScore + "/100</p>";
                html += "<p><strong>Keyword Coverage:</strong> " + Math.round(data.atsAnalysis.keywordCoverage) + "%</p>";
                if (data.atsAnalysis.criticalMissingKeywords && data.atsAnalysis.criticalMissingKeywords.length > 0) {
                    html += "<p class=\"text-warning\"><strong>Critical Missing Keywords:</strong> " + data.atsAnalysis.criticalMissingKeywords.join(", ") + "</p>";
                }
                if (data.atsAnalysis.recommendations && data.atsAnalysis.recommendations.length > 0) {
                    html += "<ul class=\"list-unstyled\">";
                    data.atsAnalysis.recommendations.forEach(function(rec) {
                        html += "<li class=\"mb-1\"><small>" + rec + "</small></li>";
                    });
                    html += "</ul>";
                }
            }

            html += "<h5 class=\"mt-3\">Tailored Resume Preview</h5>";
            html += approvalHandler.renderResumePreview(data.tailoredResume);

            html += "<div class=\"mt-4 text-center\">";
            if (data.guardrail.status === "FAIL") {
                html += "<button type=\"button\" class=\"btn btn-danger me-2\" id=\"btn-reject\">Reject - Issues to fix</button>";
                html += "<button type=\"button\" class=\"btn btn-secondary\" disabled>Approve & Generate (Disabled)</button>";
            } else {
                html += "<button type=\"button\" class=\"btn btn-success me-2\" id=\"btn-approve\">Approve & Generate</button>";
                html += "<button type=\"button\" class=\"btn btn-outline-secondary\" id=\"btn-reject\">Reject</button>";
            }
            html += "</div>";

            html += "</div></div>";

            $("#preview-content").html(html);
        },

        renderResumePreview: function(tailoredResume) {
            var html = "<div class=\"document-preview\">";
            
            if (tailoredResume.professionalSummary) {
                html += "<div class=\"doc-section\">";
                html += "<h4>Professional Summary</h4>";
                html += "<p>" + tailoredResume.professionalSummary + "</p>";
                html += "</div>";
            }

            if (tailoredResume.coreCompetencies && tailoredResume.coreCompetencies.length > 0) {
                html += "<div class=\"doc-section\">";
                html += "<h4>Core Competencies</h4>";
                html += "<p>" + tailoredResume.coreCompetencies.join(" | ") + "</p>";
                html += "</div>";
            }

            if (tailoredResume.technicalSkills && tailoredResume.technicalSkills.length > 0) {
                html += "<div class=\"doc-section\">";
                html += "<h4>Technical Skills</h4>";
                tailoredResume.technicalSkills.forEach(function(cat) {
                    if (cat.skills && cat.skills.length > 0) {
                        html += "<p><strong>" + cat.category + ":</strong> " + cat.skills.join(", ") + "</p>";
                    }
                });
                html += "</div>";
            }

            if (tailoredResume.experience && tailoredResume.experience.length > 0) {
                html += "<div class=\"doc-section\">";
                html += "<h4>Professional Experience</h4>";
                tailoredResume.experience.forEach(function(exp) {
                    html += "<p><strong>" + exp.employer + "</strong> - <strong>" + exp.title + "</strong>";
                    html += " <span class=\"text-muted\">(" + exp.startDate + " - " + (exp.isCurrent ? "Present" : exp.endDate) + ")</span></p>";
                    if (exp.bullets && exp.bullets.length > 0) {
                        html += "<ul>";
                        exp.bullets.forEach(function(bullet) {
                            html += "<li class=\"doc-bullet\">" + (bullet.tailored || bullet.original) + "</li>";
                        });
                        html += "</ul>";
                    }
                });
                html += "</div>";
            }

            html += "</div>";
            return html;
        },

        approveDocuments: function() {
            if (confirm("Are you sure you want to approve and generate documents? These will not be saved after download.")) {
                $.post("/api/tailoring/approve", { approved: true })
                    .done(function(response) {
                        if (response.success) {
                            $("#preview-content").html(
                                "<div class=\"text-center py-5\">" +
                                "<div class=\"alert alert-success\"><strong>Documents Generated!</strong></div>" +
                                "<p class=\"mb-4\">Your tailored resume and cover letter are ready for download.</p>" +
                                "<a href=\"/api/documents/resume\" class=\"btn btn-primary btn-lg me-2\" id=\"btn-download-resume\">Download Resume.docx</a>" +
                                "<a href=\"/api/documents/cover-letter\" class=\"btn btn-outline-primary btn-lg\" id=\"btn-download-cover-letter\">Download Cover Letter.docx</a>" +
                                "<div class=\"mt-4\"><a href=\"/session/logout\" class=\"btn btn-outline-secondary\">Start New Session</a></div>" +
                                "</div>"
                            );
                        } else {
                            alert("Failed to generate documents: " + response.message);
                        }
                    })
                    .fail(function() {
                        alert("Failed to approve documents");
                    });
            }
        },

        rejectDocuments: function() {
            $.post("/api/tailoring/approve", { approved: false })
                .done(function(response) {
                    if (response.success) {
                        alert("Documents rejected. You can re-upload files or start a new session.");
                        window.location.href = "/";
                    }
                })
                .fail(function() {
                    alert("Failed to reject documents");
                });
        },

        downloadResume: function() {
            window.open("/api/documents/resume", "_blank");
        },

        downloadCoverLetter: function() {
            window.open("/api/documents/cover-letter", "_blank");
        }
    };

    window.approvalHandler = approvalHandler;

    $(document).ready(function() {
        approvalHandler.init();
    });
});
