$(function() {
    var analysisHandler = {
        init: function() {
            this.bindEvents();
            this.checkAnalysisStatus();
        },

        bindEvents: function() {
            $(document).off("click", "#btn-analyze").on("click", "#btn-analyze", function() {
                analysisHandler.startAnalysis();
            });
        },

        startAnalysis: function() {
            var targetRole = $("#target-role").val();
            if (!targetRole) {
                alert("Please enter a target role");
                return;
            }

            if (!window.resumeUploaded || !window.jdUploaded) {
                alert("Please upload both resume and job description");
                return;
            }

            $.post("/api/session/start", { targetRole: targetRole })
                .done(function(response) {
                    if (response.success) {
                        $("#session-status").text("Analyzing...").removeClass("text-warning text-success").addClass("text-info");
                        
                        $.post("/api/analysis/start")
                            .done(function(resp) {
                                if (resp.success) {
                                    window.location.href = "/resume/result";
                                } else {
                                    $("#session-status").text("Failed").removeClass("text-info").addClass("text-danger");
                                    alert("Analysis failed: " + resp.message);
                                }
                            })
                            .fail(function() {
                                $("#session-status").text("Failed").removeClass("text-info").addClass("text-danger");
                                alert("Analysis request failed");
                            });
                    }
                })
                .fail(function() {
                    alert("Failed to start session");
                });
        },

        checkAnalysisStatus: function() {
            $.get("/api/analysis/status", function(response) {
                if (response.success && response.data) {
                    if (response.data.isComplete) {
                        window.location.href = "/resume/result";
                    }
                }
            });
        },

        renderProgress: function(progress) {
            var steps = [
                { id: "step-resume", name: "Resume Parsed", completed: progress.resumeParsed },
                { id: "step-jd", name: "Job Description Analyzed", completed: progress.jdParsed },
                { id: "step-skills", name: "Skills Extracted", completed: progress.skillsExtracted },
                { id: "step-matched", name: "Skills Matched", completed: progress.skillsMatched },
                { id: "step-tailoring", name: "Tailoring Resume", completed: progress.tailoringCompleted },
                { id: "step-ats", name: "ATS Validation", completed: false },
                { id: "step-guardrail", name: "Guardrail Validation", completed: progress.guardrailCompleted }
            ];

            var html = "";
            steps.forEach(function(step, index) {
                var status = step.completed ? "completed" : "active";
                var icon = step.completed ? "&#10003;" : "&#8226;";
                html += '<div class="analysis-step ' + status + '">';
                html += '<span class="step-indicator ' + status + '">' + icon + '</span>';
                html += '<span>' + step.name + '</span>';
                html += '</div>';
            });

            $("#progress-container").html(html);
        }
    };

    window.analysisHandler = analysisHandler;
});