﻿
using Microsoft.AspNetCore.Mvc;
using OnlineExam.Data;
using OnlineExam.Models;
using OnlineExam.ViewModels;

namespace OnlineExam.Controllers
{
    public class StudentAnswerController : Controller
    {

        private readonly ApplicationDbContext _context;

        //private string ConnectionString = "Server=(localdb)\\ProjectModels;Database=OnlineExam;Trusted_Connection=True;MultipleActiveResultSets=true";

        public StudentAnswerController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public IActionResult Index(int? id)
        {


            if (id == null || _context.Exams.Where(i => i.ExamId == id).Count() == 0)
            {
                return NotFound();
            }

            var model = new StudentInfo { ExamId = (int)id, NationalId = "", Name = "" };

            return View(model);
        }




        [HttpPost]
        public IActionResult AddAnswer(StudentInfo student)
        {
            var StudentAns = new Answer
            {
                ExamId = student.ExamId,
                StudentNationalId = student.NationalId,
                StudentName = student.Name,
                Score = 0
            };

            _context.Answers.Add(StudentAns);
            _context.SaveChanges();

            var questions = _context.Questions.Where(i => i.ExamId == student.ExamId).ToList();

            StudentAns = _context.Answers.Where(i => i.StudentNationalId == student.NationalId).ToArray()[0];

            int idx = 0;
            foreach (var q in questions)
            {
                var ansQ = new AnswerQuestion
                {
                    Head = q.Head,
                    a = q.a,
                    b = q.b,
                    c = q.c,
                    d = q.d,
                    AnswerId = StudentAns.AnswerId,
                    TrueAnswer = q.SelectedAnswer,
                    Index = idx

                };

                idx++;

                _context.AnswerQuestions.Add(ansQ);
            }



            _context.SaveChanges();

            IndexAndAnswerId IAndA = new IndexAndAnswerId
            {
                Index = 0,
                AnswerId = StudentAns.AnswerId
            };


            return RedirectToAction("StudentExam", IAndA);
        }

        //[HttpGet]
        public IActionResult StudentExam(IndexAndAnswerId indexx)
        {
            var question = _context.AnswerQuestions.FirstOrDefault(item => item.Index == indexx.Index && item.AnswerId == indexx.AnswerId);
            if (question == null)
            {
                return RedirectToAction(nameof(Submit), new { indexx.AnswerId });
            }
            return View(question);
        }

        [HttpPost]
        public IActionResult StudentExam(IFormCollection form)
        {
            int index;
            if (!int.TryParse(form["index"], out index))
            {
                index = 0;
            }

            int answerId = 0;

            if (int.TryParse(form["AnswerId"], out answerId))
            {


            }

            int answerIndex;
            if (int.TryParse(form["AnswerId"], out answerIndex))
            {
                var question = _context.AnswerQuestions.FirstOrDefault(q => q.Index == index && q.AnswerId == answerId);
                if (question != null)
                {
                    int selectedOption;
                    if (int.TryParse(form["SelectedAnswer"], out selectedOption))
                    {
                        question.SelectedAnswer = (byte)selectedOption;

                        _context.AnswerQuestions.Update(question);
                        _context.SaveChanges();

                    }
                }
            }

            string action = form["action"];
            if (action == "prev")
            {
                index--;
            }
            else if (action == "next")
            {
                index++;
            }

            return RedirectToAction(nameof(StudentExam), new IndexAndAnswerId { Index = index, AnswerId = answerId });
        }

        //[HttpGet]
        public IActionResult Submit(int AnswerId)
        {
            int score = 0;
            var answers = _context.AnswerQuestions.Where(i => i.AnswerId == AnswerId).ToList();


            foreach (var q in answers)
            {
                if (q.SelectedAnswer == q.TrueAnswer)
                    score++;
            }
            Answer ans = _context.Answers.Where(i => i.AnswerId == AnswerId).ToArray()[0];
            ans.Score = score;
            _context.Answers.Update(ans);
            _context.SaveChanges();
            return View(score);
        }


    }
}
